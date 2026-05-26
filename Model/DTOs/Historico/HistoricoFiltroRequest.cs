namespace Backend.DTOs.Historico
{
    public class HistoricoFiltroRequest
    {
        public string? Resultado { get; set; } // "Concluída", "Falhada", "Cancelada"
        public DateTime? De { get; set; }
        public DateTime? Ate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20; // max 100 no service
    }
}
