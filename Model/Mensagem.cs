using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Mensagens")]
    public class Mensagem
    {
        [Key] public int Id { get; set; }
        public int IdGrupo { get; set; }
        public int IdUsuario { get; set; }

        [Required] public string MensagemTexto { get; set; } = null!;
        public DateTime DataEnvio { get; set; } = DateTime.Now;
    }
}
