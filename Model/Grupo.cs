using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Grupos")]
    public class Grupo
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string NomeGrupo { get; set; } = null!;

        public int? IdMissaoVinculada { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Reputacao { get; set; } = 50.00m;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
