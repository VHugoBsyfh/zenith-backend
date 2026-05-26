using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Contratos")]
    public class Contrato
    {
        [Key] public int Id { get; set; }
        [Required] public int IdMissaoAceita { get; set; }
        [Required] public string Termos { get; set; } = null!;
        public string? Penalidades { get; set; }
        public string? Recompensas { get; set; }
        public DateTime DataGeracao { get; set; } = DateTime.Now;

        // “PDF” simulado (armazenamos bytes)
        public byte[]? VersaoPDF { get; set; }
    }
}
