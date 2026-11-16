using RuteoYa.Entidades.Common;
using RuteoYa.Entidades.Common.Enums;
using RuteoYa.Entidades.DTOs;
using RuteoYa.Entidades.Entities;
using RuteoYa.Entidades.Interfaces.Repositories;
using RuteoYa.Entidades.Interfaces.Services;

namespace RuteoYa.Logica.Services;

public class OptimizadorService : IOptimizadorService
{
    private readonly IOportunidadesRepositorio _oportunidadesRepo;
    private readonly IDistanciaService _distanciaService;

    public OptimizadorService(
        IOportunidadesRepositorio oportunidadesRepo,
        IDistanciaService distanciaService)
    {
        _oportunidadesRepo = oportunidadesRepo;
        _distanciaService = distanciaService;
    }


    public async Task<ResultadoOptimizacionDto?> ObtenerOptimizacionPorViajeAsync(int viajeId, int usuarioId)
    {
        var viaje = await _oportunidadesRepo.GetViajePorIdAsync(viajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return null;

        var capacidad = await CalcularCapacidadDisponibleAsync(viaje);

        var restricciones = new RestriccionesViajeDto
        {
            ViajeId = viaje.Id,
            Origen = viaje.OrigenDir,
            Destino = viaje.DestinoDir ?? string.Empty,
            MaxPaquetes = capacidad.PaquetesDisponibles,
            PesoMaxKg = capacidad.PesoDisponibleKg,
            DesvioMaxKm = capacidad.DesvioDisponibleKm
        };

        if (capacidad.PaquetesDisponibles <= 0 || capacidad.PesoDisponibleKg <= 0)
            return new ResultadoOptimizacionDto { ViajeId = viaje.Id, Restricciones = restricciones };

        var enviosDb = await _oportunidadesRepo.GetEnviosCompatiblesAsync(capacidad.PesoDisponibleKg);

        var enviosFiltrados = enviosDb
            .Where(e => e.UsuarioId != usuarioId && TieneDiaCompatible(viaje.ViajeDias, e.FechaLimite))
            .ToList();

        if (!enviosFiltrados.Any())
            return new ResultadoOptimizacionDto { ViajeId = viaje.Id, Restricciones = restricciones };

        List<TramoHabitual> tramosHabituales;
        var cacheRutas = new Dictionary<string, DistanciaRutaDto>();

        try
        {
            tramosHabituales = await CalcularTramosHabitualesAsync(viaje, cacheRutas);
        }
        catch
        {
            return new ResultadoOptimizacionDto { ViajeId = viaje.Id, Restricciones = restricciones };
        }

        var candidatos = new List<OportunidadDto>();
        int idx = 0;

        foreach (var e in enviosFiltrados)
        {
            try
            {
                var calculo = await CalcularDesvioYTiempoAsync(tramosHabituales, e, cacheRutas);

                if (calculo.DesvioKm > capacidad.DesvioDisponibleKm)
                    continue;

                candidatos.Add(new OportunidadDto
                {
                    EnvioId = e.Id,
                    Codigo = idx < 26 ? ((char)('A' + idx)).ToString() : $"E{idx + 1}",
                    Origen = e.OrigenDir,
                    Destino = e.DestinoDir,
                    Pago = e.Monto,
                    PesoKg = e.PesoKg,
                    DesvioKm = calculo.DesvioKm
                });
                idx++;
            }
            catch
            {
                // Si un envío puntual falla al calcular su ruta, se omite sin interrumpir el resto
            }
        }

        return CalcularMejorCombinacion(restricciones, candidatos);
    }

    public async Task<ResultadoOperacion<string>> ConfirmarCombinacionAsync(ConfirmarAsignacionDto dto, int usuarioId)
    {
        if (dto.EnvioIds == null || !dto.EnvioIds.Any())
            return ResultadoOperacion<string>.Invalido("Debe seleccionar al menos un envío.");

        var idsUnicos = dto.EnvioIds.Distinct().ToList();
        if (idsUnicos.Count != dto.EnvioIds.Count)
            return ResultadoOperacion<string>.Invalido("La lista contiene envíos repetidos.");

        var viaje = await _oportunidadesRepo.GetViajePorIdAsync(dto.ViajeId);
        if (viaje == null || viaje.UsuarioId != usuarioId)
            return ResultadoOperacion<string>.NoEncontrado("Viaje no encontrado o inactivo.");

        var envios = await _oportunidadesRepo.GetEnviosPorIdsAsync(idsUnicos);
        if (envios.Count != idsUnicos.Count)
            return ResultadoOperacion<string>.NoEncontrado("Uno o más envíos no existen.");

        if (envios.Any(e => e.UsuarioId == usuarioId))
            return ResultadoOperacion<string>.Invalido("No podés aceptar tus propios envíos.");

        if (envios.Any(e => e.Estado != (byte)EstadoEnvio.BuscandoConductor))
            return ResultadoOperacion<string>.Conflicto("Uno o más envíos ya no están disponibles.");

        if (envios.Any(e => !TieneDiaCompatible(viaje.ViajeDias, e.FechaLimite)))
            return ResultadoOperacion<string>.Invalido("Uno o más envíos no coinciden con las fechas del viaje.");

        var capacidad = await CalcularCapacidadDisponibleAsync(viaje);

        if (envios.Count > capacidad.PaquetesDisponibles)
            return ResultadoOperacion<string>.Invalido("La selección supera la cantidad máxima de paquetes disponibles del viaje.");

        if (envios.Sum(e => e.PesoKg) > capacidad.PesoDisponibleKg)
            return ResultadoOperacion<string>.Invalido("La selección supera el peso máximo disponible del viaje.");

        var detallesAsignacion = new List<AsignacionEnvioDetalleDto>();
        decimal desvioTotalKm = 0m;

        try
        {
            var cacheRutas = new Dictionary<string, DistanciaRutaDto>();
            var tramosHabituales = await CalcularTramosHabitualesAsync(viaje, cacheRutas);

            foreach (var e in envios)
            {
                var calculo = await CalcularDesvioYTiempoAsync(tramosHabituales, e, cacheRutas);
                desvioTotalKm += calculo.DesvioKm;

                detallesAsignacion.Add(new AsignacionEnvioDetalleDto
                {
                    EnvioId = e.Id,
                    DesvioKm = calculo.DesvioKm,
                    TiempoExtraMin = calculo.TiempoExtraMin,
                    Ganancia = e.Monto
                });
            }
        }
        catch
        {
            return ResultadoOperacion<string>.Invalido("No se pudo validar el desvío de la ruta seleccionada.");
        }

        if (desvioTotalKm > capacidad.DesvioDisponibleKm)
            return ResultadoOperacion<string>.Invalido("La selección supera el desvío máximo permitido del viaje.");

        var fechaViaje = ObtenerProximaFechaViaje(viaje.ViajeDias);

        var (exito, mensajeError) = await _oportunidadesRepo.AsignarEnviosViajesAsync(
            viaje.Id,
            detallesAsignacion,
            fechaViaje);

        if (!exito)
            return ResultadoOperacion<string>.Conflicto(mensajeError ?? "Uno o más envíos fueron tomados por otro conductor.");

        return ResultadoOperacion<string>.Exito("Envíos reservados con éxito.");
    }


    public ResultadoOptimizacionDto CalcularMejorCombinacion(RestriccionesViajeDto restricciones, List<OportunidadDto> candidatos)
    {
        if (candidatos == null || !candidatos.Any() || restricciones.MaxPaquetes <= 0)
            return new ResultadoOptimizacionDto { ViajeId = restricciones.ViajeId, Restricciones = restricciones };

        var validas = new List<CombinacionDto>();
        var descartadas = new List<CombinacionDto>();

        GenerarCombosRecursivo(candidatos, restricciones, 0, new List<OportunidadDto>(), 0m, 0m, validas, descartadas);

        return ArmarResultado(restricciones, validas, descartadas);
    }

    private void GenerarCombosRecursivo(
        List<OportunidadDto> candidatos,
        RestriccionesViajeDto restricciones,
        int inicio,
        List<OportunidadDto> comboActual,
        decimal pesoActual,
        decimal desvioActual,
        List<CombinacionDto> validas,
        List<CombinacionDto> descartadas)
    {
        if (comboActual.Count > 0)
            EvaluarYGuardar(new List<OportunidadDto>(comboActual), restricciones, validas, descartadas);

        if (comboActual.Count >= restricciones.MaxPaquetes ||
            pesoActual > restricciones.PesoMaxKg ||
            desvioActual > restricciones.DesvioMaxKm)
        {
            return;
        }

        for (int i = inicio; i < candidatos.Count; i++)
        {
            comboActual.Add(candidatos[i]);
            GenerarCombosRecursivo(
                candidatos,
                restricciones,
                i + 1,
                comboActual,
                pesoActual + candidatos[i].PesoKg,
                desvioActual + candidatos[i].DesvioKm,
                validas,
                descartadas);
            comboActual.RemoveAt(comboActual.Count - 1);
        }
    }

    private void EvaluarYGuardar(
        List<OportunidadDto> envios,
        RestriccionesViajeDto restricciones,
        List<CombinacionDto> validas,
        List<CombinacionDto> descartadas)
    {
        var combo = new CombinacionDto
        {
            Envios = envios,
            GananciaTotal = envios.Sum(e => e.Pago),
            PesoTotalKg = envios.Sum(e => e.PesoKg),
            DesvioTotalKm = envios.Sum(e => e.DesvioKm),
            EsValida = true
        };

        if (combo.PesoTotalKg > restricciones.PesoMaxKg)
        {
            combo.EsValida = false;
            combo.MotivoDescarte = $"Supera {restricciones.PesoMaxKg} kg";
            if (descartadas.Count < 20) descartadas.Add(combo);
        }
        else if (combo.DesvioTotalKm > restricciones.DesvioMaxKm)
        {
            combo.EsValida = false;
            combo.MotivoDescarte = $"Supera {restricciones.DesvioMaxKm} km";
            if (descartadas.Count < 20) descartadas.Add(combo);
        }
        else
        {
            validas.Add(combo);
        }
    }

    private ResultadoOptimizacionDto ArmarResultado(
        RestriccionesViajeDto restricciones,
        List<CombinacionDto> validas,
        List<CombinacionDto> descartadas,
        string criterio = "ganancias")
    {
        List<CombinacionDto> validasOrdenadas = criterio.ToLower() switch
        {
            "desvios" => validas
                .OrderBy(c => c.DesvioTotalKm)
                .ThenByDescending(c => c.GananciaTotal)
                .ToList(),
            "equilibrio" => validas
                .OrderByDescending(c => c.GananciaTotal / (c.DesvioTotalKm + 1m))
                .ThenByDescending(c => c.GananciaTotal)
                .ThenBy(c => c.DesvioTotalKm)
                .ToList(),
            _ => validas
                .OrderByDescending(c => c.GananciaTotal)
                .ThenBy(c => c.DesvioTotalKm)
                .ToList()
        };

        return new ResultadoOptimizacionDto
        {
            ViajeId = restricciones.ViajeId,
            Restricciones = restricciones,
            CombinacionRecomendada = validasOrdenadas.FirstOrDefault(),
            OtrasCombinacionesValidas = validasOrdenadas.Skip(1).ToList(),
            CombinacionesDescartadas = descartadas.OrderByDescending(c => c.GananciaTotal).Take(5).ToList()
        };
    }

    private ResultadoOptimizacionDto CalcularMejorCombinacionConCriterio(
        RestriccionesViajeDto restricciones,
        List<OportunidadDto> candidatos,
        string criterio)
    {
        if (candidatos == null || !candidatos.Any() || restricciones.MaxPaquetes <= 0)
            return new ResultadoOptimizacionDto { ViajeId = restricciones.ViajeId, Restricciones = restricciones };

        var validas = new List<CombinacionDto>();
        var descartadas = new List<CombinacionDto>();

        GenerarCombosRecursivo(candidatos, restricciones, 0, new List<OportunidadDto>(), 0m, 0m, validas, descartadas);

        return ArmarResultado(restricciones, validas, descartadas, criterio);
    }

    // =========================================================================
    // 4. POTENCIAL DE GANANCIAS MENSUAL (PANTALLA 14 - REQUISITO PARA 10)
    // =========================================================================
    public async Task<ResultadoOperacion<PotencialMensualDto>> CalcularPotencialMensualAsync(
        int usuarioId, int anio, int mes, string criterio = "ganancias")
    {
        if (mes < 1 || mes > 12 || anio < 2000 || anio > 2100)
            return ResultadoOperacion<PotencialMensualDto>.Invalido("Mes o año inválido.");

        string criterioNorm = string.IsNullOrWhiteSpace(criterio) ? "ganancias" : criterio.Trim().ToLower();
        if (criterioNorm != "ganancias" && criterioNorm != "desvios" && criterioNorm != "equilibrio")
            return ResultadoOperacion<PotencialMensualDto>.Invalido("Criterio inválido. Use: ganancias, desvios o equilibrio.");

        var gananciaEfectiva = await _oportunidadesRepo.GetGananciaEfectivaMesAsync(usuarioId, anio, mes);

        var resultadoDto = new PotencialMensualDto
        {
            Anio = anio,
            Mes = mes,
            Criterio = criterioNorm,
            GananciaEfectivaObtenida = gananciaEfectiva
        };

        var primerDiaMes = new DateTime(anio, mes, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);
        var fechaDesde = primerDiaMes < DateTime.Today ? DateTime.Today : primerDiaMes;

        // Si es un mes ya pasado o sin viajes activos, devolvemos solo lo efectivamente ganado
        var viajes = await _oportunidadesRepo.GetViajesActivosDeUsuarioAsync(usuarioId);
        if (fechaDesde > ultimoDiaMes || !viajes.Any())
            return ResultadoOperacion<PotencialMensualDto>.Exito(resultadoDto);

        var maxPesoViajes = viajes.Max(v => v.PesoMaxKg);
        var enviosDb = await _oportunidadesRepo.GetEnviosCompatiblesAsync(maxPesoViajes);
        var enviosDisponibles = enviosDb
            .Where(e => e.UsuarioId != usuarioId && e.FechaLimite.Date >= fechaDesde)
            .ToList();

        // Precalcular envíos ya asignados y desvíos por plantilla de viaje (una sola vez por viaje)
        var cacheRutas = new Dictionary<string, DistanciaRutaDto>();
        var asignadosPorViaje = new Dictionary<int, List<Envio>>();
        var candidatosPorViaje = new Dictionary<int, List<CandidatoMensual>>();

        foreach (var viaje in viajes)
        {
            asignadosPorViaje[viaje.Id] = await _oportunidadesRepo.GetEnviosAsignadosAViajeAsync(viaje.Id);

            var listaCandidatos = new List<CandidatoMensual>();
            try
            {
                var tramosHabituales = await CalcularTramosHabitualesAsync(viaje, cacheRutas);
                int idx = 0;

                foreach (var e in enviosDisponibles.Where(x => x.PesoKg <= viaje.PesoMaxKg))
                {
                    try
                    {
                        var calculo = await CalcularDesvioYTiempoAsync(tramosHabituales, e, cacheRutas);
                        if (calculo.DesvioKm > viaje.DesvioMaxKm)
                            continue;

                        listaCandidatos.Add(new CandidatoMensual
                        {
                            FechaLimite = e.FechaLimite,
                            Oportunidad = new OportunidadDto
                            {
                                EnvioId = e.Id,
                                Codigo = idx < 26 ? ((char)('A' + idx)).ToString() : $"E{idx + 1}",
                                Origen = e.OrigenDir,
                                Destino = e.DestinoDir,
                                Pago = e.Monto,
                                PesoKg = e.PesoKg,
                                DesvioKm = calculo.DesvioKm
                            }
                        });
                        idx++;
                    }
                    catch
                    {
                        // Se omite el envío puntual si falla el cálculo de su ruta
                    }
                }
            }
            catch
            {
                // Se omite el viaje si no se puede trazar su ruta habitual
            }

            candidatosPorViaje[viaje.Id] = listaCandidatos;
        }

        // Generar ocurrencias futuras del mes por fecha cronológica
        var ocurrencias = new List<OcurrenciaViaje>();
        for (var fecha = fechaDesde; fecha <= ultimoDiaMes; fecha = fecha.AddDays(1))
        {
            foreach (var viaje in viajes)
            {
                if (viaje.ViajeDias != null && viaje.ViajeDias.Any(d => (DayOfWeek)d.DiaSemana == fecha.DayOfWeek))
                {
                    ocurrencias.Add(new OcurrenciaViaje { Viaje = viaje, Fecha = fecha });
                }
            }
        }

        ocurrencias = ocurrencias
            .OrderBy(o => o.Fecha)
            .ThenBy(o => o.Viaje.HoraSalida)
            .ToList();

        var enviosUsados = new HashSet<int>();
        var gananciaPorRecorrido = new Dictionary<string, decimal>();

        foreach (var ocurrencia in ocurrencias)
        {
            var viaje = ocurrencia.Viaje;
            var yaAsignadosFecha = asignadosPorViaje[viaje.Id]
                .Where(e => e.Asignacion != null && e.Asignacion.FechaViaje.Date == ocurrencia.Fecha.Date)
                .ToList();

            int paquetesRestantes = Math.Max(0, viaje.MaxPaquets - yaAsignadosFecha.Count);
            decimal pesoRestante = Math.Max(0m, viaje.PesoMaxKg - yaAsignadosFecha.Sum(e => e.PesoKg));
            decimal desvioRestante = Math.Max(0m, viaje.DesvioMaxKm - yaAsignadosFecha
                .Select(e => e.Asignacion!)
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .Sum(a => a.DesvioKm));

            var restricciones = new RestriccionesViajeDto
            {
                ViajeId = viaje.Id,
                Origen = viaje.OrigenDir,
                Destino = viaje.DestinoDir ?? string.Empty,
                MaxPaquetes = paquetesRestantes,
                PesoMaxKg = pesoRestante,
                DesvioMaxKm = desvioRestante
            };

            var candidatosFecha = candidatosPorViaje[viaje.Id]
                .Where(c => !enviosUsados.Contains(c.Oportunidad.EnvioId)
                         && c.FechaLimite.Date >= ocurrencia.Fecha.Date
                         && c.Oportunidad.PesoKg <= pesoRestante
                         && c.Oportunidad.DesvioKm <= desvioRestante)
                .Select(c => c.Oportunidad)
                .ToList();

            var optimizacion = CalcularMejorCombinacionConCriterio(restricciones, candidatosFecha, criterioNorm);
            var recomendada = optimizacion.CombinacionRecomendada;

            var detalle = new ViajePotencialDetalleDto
            {
                ViajeId = viaje.Id,
                Recorrido = $"{viaje.OrigenDir} → {viaje.DestinoDir}",
                Fecha = ocurrencia.Fecha
            };

            if (recomendada != null)
            {
                detalle.GananciaPotencial = recomendada.GananciaTotal;
                detalle.DesvioTotalKm = recomendada.DesvioTotalKm;
                detalle.PaquetesSugeridos = recomendada.Envios;

                foreach (var env in recomendada.Envios)
                    enviosUsados.Add(env.EnvioId);

                resultadoDto.GananciaPotencialTotal += recomendada.GananciaTotal;
                resultadoDto.PaquetesPosibles += recomendada.CantidadPaquetes;

                if (!gananciaPorRecorrido.ContainsKey(detalle.Recorrido))
                    gananciaPorRecorrido[detalle.Recorrido] = 0m;
                gananciaPorRecorrido[detalle.Recorrido] += recomendada.GananciaTotal;
            }

            resultadoDto.DetalleViajes.Add(detalle);
        }

        resultadoDto.ViajesAnalizados = ocurrencias.Count;
        resultadoDto.PromedioPorViaje = resultadoDto.ViajesAnalizados > 0
            ? Math.Round(resultadoDto.GananciaPotencialTotal / resultadoDto.ViajesAnalizados, 2)
            : 0m;

        resultadoDto.GananciaPorSemana = resultadoDto.DetalleViajes
            .GroupBy(d => ((d.Fecha.Day - 1) / 7) + 1)
            .OrderBy(g => g.Key)
            .Select(g => new GananciaSemanalDto
            {
                Semana = g.Key,
                GananciaPotencial = g.Sum(x => x.GananciaPotencial),
                Viajes = g.Count(),
                Paquetes = g.Sum(x => x.PaquetesSugeridos.Count)
            })
            .ToList();

        if (gananciaPorRecorrido.Any())
        {
            var mejor = gananciaPorRecorrido.OrderByDescending(x => x.Value).First();
            resultadoDto.MejorRecorrido = mejor.Key;
            resultadoDto.MejorRecorridoGanancia = mejor.Value;
        }

        return ResultadoOperacion<PotencialMensualDto>.Exito(resultadoDto);
    }

    private class CandidatoMensual
    {
        public DateTime FechaLimite { get; set; }
        public OportunidadDto Oportunidad { get; set; } = new();
    }

    private class OcurrenciaViaje
    {
        public Viaje Viaje { get; set; } = new();
        public DateTime Fecha { get; set; }
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Clases Auxiliares
    // -----------------------------------------------------------------------------------------------------------------


    private class TramoHabitual
    {
        public decimal OrigLat { get; set; }
        public decimal OrigLng { get; set; }
        public decimal DestLat { get; set; }
        public decimal DestLng { get; set; }
        public decimal DistanciaKm { get; set; }
        public int DuracionMin { get; set; }
    }

    private class CapacidadViaje
    {
        public int PaquetesDisponibles { get; set; }
        public decimal PesoDisponibleKg { get; set; }
        public decimal DesvioDisponibleKm { get; set; }
    }

    private class ResultadoDesvio
    {
        public decimal DesvioKm { get; set; }
        public int TiempoExtraMin { get; set; }
    }

    private async Task<DistanciaRutaDto> MedirPorCoordenadasAsync(
        decimal lat1, decimal lng1, decimal lat2, decimal lng2,
        Dictionary<string, DistanciaRutaDto> cache)
    {
        string clave = $"{lat1},{lng1}->{lat2},{lng2}";
        if (cache.ContainsKey(clave))
            return cache[clave];

        var ruta = await _distanciaService.CalcularPorCoordenadasAsync(
            (double)lat1, (double)lng1, (double)lat2, (double)lng2);

        cache[clave] = ruta;
        return ruta;
    }

    private async Task<List<TramoHabitual>> CalcularTramosHabitualesAsync(
        Viaje viaje,
        Dictionary<string, DistanciaRutaDto> cache)
    {
        var latitudes = new List<decimal> { viaje.OrigenLat };
        var longitudes = new List<decimal> { viaje.OrigenLng };

        if (viaje.Paradas != null && viaje.Paradas.Count > 0)
        {
            foreach (var p in viaje.Paradas.OrderBy(p => p.Orden))
            {
                latitudes.Add(p.Lat);
                longitudes.Add(p.Lng);
            }
        }

        latitudes.Add(viaje.DestinoLat);
        longitudes.Add(viaje.DestinoLng);

        var tramos = new List<TramoHabitual>();
        for (int i = 0; i < latitudes.Count - 1; i++)
        {
            var ruta = await MedirPorCoordenadasAsync(
                latitudes[i], longitudes[i],
                latitudes[i + 1], longitudes[i + 1],
                cache);

            tramos.Add(new TramoHabitual
            {
                OrigLat = latitudes[i],
                OrigLng = longitudes[i],
                DestLat = latitudes[i + 1],
                DestLng = longitudes[i + 1],
                DistanciaKm = (decimal)ruta.DistanciaKm,
                DuracionMin = ruta.DuracionMinutos
            });
        }

        return tramos;
    }

    private async Task<ResultadoDesvio> CalcularDesvioYTiempoAsync(
        List<TramoHabitual> tramosHabituales,
        Envio envio,
        Dictionary<string, DistanciaRutaDto> cache)
    {
        var tramoEnvio = await MedirPorCoordenadasAsync(
            envio.OrigenLat, envio.OrigenLng,
            envio.DestinoLat, envio.DestinoLng,
            cache);

        decimal mejorDesvio = decimal.MaxValue;
        int mejorTiempoExtra = int.MaxValue;

        for (int i = 0; i < tramosHabituales.Count; i++)
        {
            for (int j = i; j < tramosHabituales.Count; j++)
            {
                decimal desvio;
                int tiempoExtra;

                if (i == j)
                {
                    var t1 = await MedirPorCoordenadasAsync(
                        tramosHabituales[i].OrigLat, tramosHabituales[i].OrigLng,
                        envio.OrigenLat, envio.OrigenLng, cache);
                    var t3 = await MedirPorCoordenadasAsync(
                        envio.DestinoLat, envio.DestinoLng,
                        tramosHabituales[i].DestLat, tramosHabituales[i].DestLng, cache);

                    desvio = Math.Max(0m, (decimal)(t1.DistanciaKm + tramoEnvio.DistanciaKm + t3.DistanciaKm) - tramosHabituales[i].DistanciaKm);
                    tiempoExtra = Math.Max(0, (t1.DuracionMinutos + tramoEnvio.DuracionMinutos + t3.DuracionMinutos) - tramosHabituales[i].DuracionMin);
                }
                else
                {
                    var tOrig1 = await MedirPorCoordenadasAsync(
                        tramosHabituales[i].OrigLat, tramosHabituales[i].OrigLng,
                        envio.OrigenLat, envio.OrigenLng, cache);
                    var tOrig2 = await MedirPorCoordenadasAsync(
                        envio.OrigenLat, envio.OrigenLng,
                        tramosHabituales[i].DestLat, tramosHabituales[i].DestLng, cache);

                    var tDest1 = await MedirPorCoordenadasAsync(
                        tramosHabituales[j].OrigLat, tramosHabituales[j].OrigLng,
                        envio.DestinoLat, envio.DestinoLng, cache);
                    var tDest2 = await MedirPorCoordenadasAsync(
                        envio.DestinoLat, envio.DestinoLng,
                        tramosHabituales[j].DestLat, tramosHabituales[j].DestLng, cache);

                    var desvioI = (decimal)(tOrig1.DistanciaKm + tOrig2.DistanciaKm) - tramosHabituales[i].DistanciaKm;
                    var desvioJ = (decimal)(tDest1.DistanciaKm + tDest2.DistanciaKm) - tramosHabituales[j].DistanciaKm;

                    var tiempoI = (tOrig1.DuracionMinutos + tOrig2.DuracionMinutos) - tramosHabituales[i].DuracionMin;
                    var tiempoJ = (tDest1.DuracionMinutos + tDest2.DuracionMinutos) - tramosHabituales[j].DuracionMin;

                    desvio = Math.Max(0m, desvioI + desvioJ);
                    tiempoExtra = Math.Max(0, tiempoI + tiempoJ);
                }

                if (desvio < mejorDesvio)
                {
                    mejorDesvio = desvio;
                    mejorTiempoExtra = tiempoExtra;
                }
            }
        }

        return new ResultadoDesvio
        {
            DesvioKm = Math.Round(mejorDesvio, 2),
            TiempoExtraMin = mejorTiempoExtra
        };
    }

    private async Task<CapacidadViaje> CalcularCapacidadDisponibleAsync(Viaje viaje)
    {
        var yaAsignados = await _oportunidadesRepo.GetEnviosAsignadosAViajeAsync(viaje.Id);

        int paquetesUsados = yaAsignados.Count;
        decimal pesoUsado = yaAsignados.Sum(e => e.PesoKg);
        decimal desvioUsado = yaAsignados
            .Where(e => e.Asignacion != null)
            .Select(e => e.Asignacion!)
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .Sum(a => a.DesvioKm);

        return new CapacidadViaje
        {
            PaquetesDisponibles = Math.Max(0, viaje.MaxPaquets - paquetesUsados),
            PesoDisponibleKg = Math.Max(0m, viaje.PesoMaxKg - pesoUsado),
            DesvioDisponibleKm = Math.Max(0m, viaje.DesvioMaxKm - desvioUsado)
        };
    }

    private static bool TieneDiaCompatible(ICollection<ViajeDia> viajeDias, DateTime fechaLimite)
    {
        if (fechaLimite.Date < DateTime.Today) return false;
        if (viajeDias == null || viajeDias.Count == 0) return true;

        var dias = viajeDias.Select(d => (DayOfWeek)d.DiaSemana).ToList();
        var tope = fechaLimite.Date < DateTime.Today.AddDays(6) ? fechaLimite.Date : DateTime.Today.AddDays(6);

        for (var fecha = DateTime.Today; fecha <= tope; fecha = fecha.AddDays(1))
        {
            if (dias.Contains(fecha.DayOfWeek))
                return true;
        }
        return false;
    }

    private static DateTime ObtenerProximaFechaViaje(ICollection<ViajeDia> viajeDias)
    {
        if (viajeDias == null || viajeDias.Count == 0) return DateTime.Today;

        var dias = viajeDias.Select(d => (DayOfWeek)d.DiaSemana).ToList();
        for (int i = 0; i < 7; i++)
        {
            var fecha = DateTime.Today.AddDays(i);
            if (dias.Contains(fecha.DayOfWeek))
                return fecha;
        }
        return DateTime.Today;
    }

    public async Task<List<ViajeResumenDto>> ObtenerViajesActivosAsync(int usuarioId)
    {
        var viajes = await _oportunidadesRepo.GetViajesActivosDeUsuarioAsync(usuarioId);

        return viajes.Select(v => new ViajeResumenDto
        {
            Id = v.Id,
            OrigenDir = v.OrigenDir,
            DestinoDir = v.DestinoDir ?? string.Empty
        }).ToList();
    }
}