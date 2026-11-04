namespace RuteoYa.Web.Models
{
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public int Id { get; set; }
        public string NombreApellido { get; set; }

        public TokenResponseDto(string token, int id, string nombreApellido)
        {
            Token = token;
            Id = id;
            NombreApellido = nombreApellido;
        }
    }

}
