using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("HistoricoMissoes")]
    public class HistoricoMissao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdMissaoAceita { get; set; }

        [Required]
        [MaxLength(50)]
        public string Resultado { get; set; } = "Concluída"; // Concluída, Falhada, Cancelada

        public decimal? AvaliacaoRecebida { get; set; }
        [MaxLength(255)]
        public string? Justificativa { get; set; }

        public DateTime DataRegistro { get; set; } = DateTime.Now;
    }
}
