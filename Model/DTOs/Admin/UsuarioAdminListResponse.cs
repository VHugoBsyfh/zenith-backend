namespace Backend.DTOs.Admin
{
    public class UsuarioAdminListResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Classe { get; set; } = null!;
        public int Nivel { get; set; }
        public string TipoUsuario { get; set; } = null!;
        public decimal Reputacao { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
