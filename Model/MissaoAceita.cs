using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("MissoesAceitas")]
    public class MissaoAceita
    {
        [Key] public int Id { get; set; }

        [Required] public int IdMissao { get; set; }
        public int? IdGrupo { get; set; }   // grupo OU
        public int? IdUsuario { get; set; } // solo

        public DateTime DataAceite { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string StatusMissao { get; set; } = "Em andamento"; // Em andamento, Concluída, Falhada, Cancelada

        public DateTime? DataConclusao { get; set; }
        public string? MotivoCancelamento { get; set; }
        public bool PenalidadeAplicada { get; set; } = false;
    }
}
