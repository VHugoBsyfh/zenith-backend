namespace Backend.DTOs.Rankings
{
    public class RankingUsuarioItem
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = null!;
        public string Classe { get; set; } = null!;
        public int Nivel { get; set; }
        public decimal Reputacao { get; set; }

        public int MissoesConcluidas { get; set; }
        public int MissoesTotais { get; set; }
        public decimal TaxaSucesso { get; set; } // 0..1
        public DateTime? UltimaAtividade { get; set; }
    }
}
