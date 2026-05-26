namespace Backend.DTOs.Historico
{
    public class HistoricoItemResponse
    {
        public int IdHistorico { get; set; }
        public int IdMissaoAceita { get; set; }
        public int IdMissao { get; set; }
        public string TituloMissao { get; set; } = null!;
        public string Resultado { get; set; } = null!;            // Concluída, Falhada, Cancelada
        public decimal? AvaliacaoRecebida { get; set; }
        public string? Justificativa { get; set; }
        public DateTime DataRegistro { get; set; }
        public int IdCriador { get; set; }
        public string NomeCriador { get; set; } = null!;
    }
}
