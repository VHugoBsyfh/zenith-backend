namespace Backend.DTOs
{
    public class AvaliacaoResponse
    {
        public int Id { get; set; }
        public int IdAvaliador { get; set; }
        public int IdAvaliado { get; set; }
        public int IdMissaoAceita { get; set; }
        public decimal Nota { get; set; }
        public string? Justificativa { get; set; }
        public DateTime DataAvaliacao { get; set; }
    }
}
