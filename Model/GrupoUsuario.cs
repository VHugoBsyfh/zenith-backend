using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("GrupoUsuarios")]
    public class GrupoUsuario
    {
        public int IdGrupo { get; set; }
        public int IdUsuario { get; set; }
    }
}
