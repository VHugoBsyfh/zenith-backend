namespace Backend.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = null!;
        public string TipoUsuario { get; set; } = null!;
    }
}
