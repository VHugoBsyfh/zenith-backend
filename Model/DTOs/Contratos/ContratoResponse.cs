namespace Backend.DTOs.Contratos
{
    public class ContratoResponse
    {
        public int Id { get; set; }
        public int IdMissaoAceita { get; set; }
        public string Termos { get; set; } = null!;
        public string? Penalidades { get; set; }
        public string? Recompensas { get; set; }
        public DateTime DataGeracao { get; set; }
        public bool TemArquivo { get; set; }
    }
}
