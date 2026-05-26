namespace Backend.DTOs
{
    public class MessageResponse
    {
        public int Id { get; set; }
        public int IdGrupo { get; set; }
        public int AutorId { get; set; }
        public string AutorNome { get; set; } = null!;
        public string Conteudo { get; set; } = null!;
        public DateTime DataEnvio { get; set; }
    }
}
