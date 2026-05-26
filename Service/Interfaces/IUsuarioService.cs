using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<UserResponse> RegistrarAsync(RegisterUserRequest request);
    }
}
