using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Missoes")]
    public class Missao
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Titulo { get; set; } = null!;

        [Required]
        public string Descricao { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Localizacao { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Recompensa { get; set; }

        [Required, MaxLength(50)]
        public string TipoMissao { get; set; } = null!;

        [Required]
        public int NivelMinimo { get; set; }

        [MaxLength(50)]
        public string? ClassePreferida { get; set; }

        //[Required]
        public int IdCriador { get; set; }
        //[Required]
        public int? IdAventureiro { get; set; }
        public int? IdGrupo { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } = "Disponível";

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool PagamentoRealizado { get; set; } = false; 
    }
}
