using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Avaliacoes")]
    public class Avaliacao
    {
        [Key] public int Id { get; set; }
        [Required] public int IdAvaliador { get; set; }
        [Required] public int IdAvaliado { get; set; }
        [Required] public int IdMissaoAceita { get; set; }

        [Range(0, 5)]
        public decimal Nota { get; set; }

        [MaxLength(255)]
        public string? Justificativa { get; set; }

        public DateTime DataAvaliacao { get; set; } = DateTime.Now;
    }
}
