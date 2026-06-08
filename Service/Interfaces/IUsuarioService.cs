using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<UserResponse> RegistrarAsync(RegisterUserRequest request);
        Task AtualizarNivelAsync(int idUsuario, int novoNivel);
        Task<List<UsuarioResponse>> ListarUsuariosAsync(string? role);
    }
}
