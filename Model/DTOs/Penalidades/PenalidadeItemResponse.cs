namespace Backend.DTOs.Penalidades
{
    public class PenalidadeItemResponse
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdMissaoAceita { get; set; }
        public string TipoPenalidade { get; set; } = null!;
        public decimal? ValorPenalidade { get; set; }
        public int? DuracaoBloqueioDias { get; set; }
        public string? Justificativa { get; set; }
        public DateTime DataAplicacao { get; set; }
    }
}
