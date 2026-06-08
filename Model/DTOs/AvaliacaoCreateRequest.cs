using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class AvaliacaoCreateRequest
    {
        [Required] public int IdMissao { get; set; }
        [Required] public int IdAvaliado { get; set; }
        [Range(0,5)] public decimal Nota { get; set; }
        [MaxLength(255)] public string? Justificativa { get; set; }
    }
}
