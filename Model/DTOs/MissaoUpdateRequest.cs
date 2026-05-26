using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class MissaoUpdateRequest
    {
        [Required, MaxLength(100)]
        public string Titulo { get; set; } = null!;

        [Required]
        public string Descricao { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Localizacao { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal Recompensa { get; set; }

        [Required, MaxLength(50)]
        public string TipoMissao { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int NivelMinimo { get; set; }

        [MaxLength(50)]
        public string? ClassePreferida { get; set; }
    }
}
