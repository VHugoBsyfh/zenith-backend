using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Penalidades")]
    public class Penalidade
    {
        [Key] public int Id { get; set; }
        [Required] public int IdUsuario { get; set; }
        [Required] public int IdMissaoAceita { get; set; }

        [Required, MaxLength(50)]
        public string TipoPenalidade { get; set; } = "ReducaoReputacao";

        public decimal? ValorPenalidade { get; set; } // ex: -1.5
        public int? DuracaoBloqueioDias { get; set; } // se quiser simular bloqueio
        [MaxLength(255)] public string? Justificativa { get; set; }
        public DateTime DataAplicacao { get; set; } = DateTime.Now;
    }
}
