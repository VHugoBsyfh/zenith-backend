using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CancelarMissaoRequest
    {
        [Required, MaxLength(255)]
        public string Motivo { get; set; } = null!;
        public decimal ReputacaoPerdida { get; set; } = 1.0m; // default
        public int? BloqueioDias { get; set; } = null;        // opcional
    }
}
