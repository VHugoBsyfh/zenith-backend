namespace Backend.DTOs
{
    public class                               RegisterUserRequest
    {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!;
        public string Classe { get; set; } = null!;
        public string TipoUsuario { get; set; } = null!;
    }
}
