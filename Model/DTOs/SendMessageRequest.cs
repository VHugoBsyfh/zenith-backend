using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class SendMessageRequest
    {
        [Required, MinLength(1)]
        public string Conteudo { get; set; } = null!;
    }
}
