namespace Backend.DTOs.Missoes
{
    public class MissaoFiltroRequest
    {
        public string? Tipo { get; set; }
        public string? Localizacao { get; set; }
        public string? ClassePreferida { get; set; }
        public decimal? RecompensaMin { get; set; }
        public decimal? RecompensaMax { get; set; }
        public int? NivelMaxUsuario { get; set; }
        public string? Status { get; set; } = "Disponível"; // padrão
        public string? SortBy { get; set; } = "data";       // data|recompensa|nivel
        public string? SortDir { get; set; } = "desc";      // asc|desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
