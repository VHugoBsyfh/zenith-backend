using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Notificacoes")]
    public class Notificacao
    {
        [Key] public int Id { get; set; }
        [Required] public int IdUsuario { get; set; }

        // Tipo livre para filtros: ConviteGrupo, MissaoMatch, StatusMissao, Sistema...
        [Required, MaxLength(40)]
        public string Tipo { get; set; } = null!;

        // Título curto e corpo descritivo
        [Required, MaxLength(120)]
        public string Titulo { get; set; } = null!;
        [Required]
        public string Mensagem { get; set; } = null!;

        // (Opcional) payload para deep-link (ex.: /missoes/123)
        [MaxLength(200)]
        public string? Link { get; set; }

        public bool Lida { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
