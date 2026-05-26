namespace Backend.DTOs.Rankings
{
    public class RankingGrupoItem
    {
        public int GrupoId { get; set; }
        public string NomeGrupo { get; set; } = null!;
        public int Membros { get; set; }

        public int MissoesConcluidas { get; set; }
        public int MissoesTotais { get; set; }
        public decimal TaxaSucesso { get; set; } // 0..1
        public DateTime? UltimaAtividade { get; set; }
    }
}
