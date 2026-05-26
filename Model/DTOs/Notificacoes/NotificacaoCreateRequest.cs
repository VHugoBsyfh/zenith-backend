using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Notificacoes
{
    public class NotificacaoCreateRequest
    {
        [Required] public int IdUsuario { get; set; }
        [Required] public string Tipo { get; set; } = null!;
        [Required] public string Titulo { get; set; } = null!;
        [Required] public string Mensagem { get; set; } = null!;
        public string? Link { get; set; }
    }
}
