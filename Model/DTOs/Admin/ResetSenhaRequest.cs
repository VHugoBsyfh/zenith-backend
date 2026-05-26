using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Admin
{
    public class ResetSenhaRequest
    {
        [Required] public string NovaSenha { get; set; } = null!;
    }
}
