# Servicio de Rutas y Distancias

Este servicio permite calcular coordenadas geográficas (lat/lng) y distancias entre dos direcciones.

## Interfaces disponibles

### `IGeocoderService`
Convierte una dirección textual en coordenadas geográficas.

```csharp
public interface IGeocoderService
{
    Task<GeocoderDto> GeocodeAsync(string direccion);
}
```

**Retorna:**
```csharp
public class GeocoderDto
{
    public string Direccion { get; set; }
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}
```

### `IDistanciaService`
Calcula la distancia y duración de la ruta óptima en auto entre dos direcciones.

```csharp
public interface IDistanciaService
{
    Task<DistanciaRutaDto> CalcularAsync(string origenDireccion, string destinoDireccion);
}
```

**Retorna:**
```csharp
public class DistanciaRutaDto
{
    public GeocoderDto Origen { get; set; }
    public GeocoderDto Destino { get; set; }
    public double DistanciaKm { get; set; }
    public int DuracionMinutos { get; set; }
}
```

---

## Cómo usarlo

### 1. Inyectar el servicio

```csharp
public class ViajesService
{
    private readonly IDistanciaService _distanciaService;

    public ViajesService(IDistanciaService distanciaService)
    {
        _distanciaService = distanciaService;
    }
}
```

### 2. Calcular coordenadas y distancia

```csharp
public async Task<Viaje> CrearViaje(CrearViajeDto dto)
{
    // Calcular automáticamente lat/lng y distancia
    var ruta = await _distanciaService.CalcularAsync(
        "Av. Corrientes 1000, Buenos Aires", 
        "Av. Santa Fe 2000, Buenos Aires"
    );

    var viaje = new Viaje
    {
        OrigenDir = dto.OrigenDir,
        OrigenLat = ruta.Origen.Lat,      // coordenadas calculadas
        OrigenLng = ruta.Origen.Lng,
        DestinoLat = ruta.Destino.Lat,
        DestinoLng = ruta.Destino.Lng,
        DistanciaMin = (int)ruta.DistanciaKm,  // distancia en km
        DuracionEstimada = ruta.DuracionMinutos // duración en minutos
    };

    return await _repository.CrearAsync(viaje);
}
```

### 3. Solo geocodificar (sin calcular distancia)

```csharp
public async Task<Parada> CrearParada(string direccion)
{
    var geocoded = await _geocoderService.GeocodeAsync(direccion);

    return new Parada
    {
        Direccion = geocoded.Direccion,
        Lat = geocoded.Lat,
        Lng = geocoded.Lng
    };
}
```

---

## Implementación actual

**Actualmente usa implementaciones MOCK** que devuelven valores fijos:
- `MockGeocoderService`: coordenadas de Buenos Aires
- `MockDistanciaService`: 12.5 km / 25 minutos

### Próximos pasos (integración con APIs reales)

Cuando se integren APIs reales de geocodificación y rutas, **no se necesita cambiar ningún código que use estos servicios**, solo reemplazar las implementaciones mock:

**Opciones de APIs gratuitas:**
- **Geocodificación**: Nominatim (OpenStreetMap), Google Geocoding API
- **Rutas/Distancia**: OSRM (Open Source Routing Machine), Google Directions API

La migración será transparente gracias a la inyección de dependencias.

---

## Registro en DI

El servicio ya está registrado en `RuteoYa.Logica/DependencyInjection.cs`:

```csharp
services.AddScoped<IGeocoderService, MockGeocoderService>();
services.AddScoped<IDistanciaService, MockDistanciaService>();
```

Solo asegurate de llamar `AddInfrastructureServices()` en tu `Program.cs`.
