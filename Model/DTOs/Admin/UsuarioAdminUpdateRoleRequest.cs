using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Admin
{
    public class UsuarioAdminUpdateRoleRequest
    {
        [Required] public string NovoTipoUsuario { get; set; } = null!; // "Aventureiro" | "Criador" | "Admin"
    }
}
