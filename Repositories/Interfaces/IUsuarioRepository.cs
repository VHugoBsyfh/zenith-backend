using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<bool> EmailExisteAsync(string email);
        Task<Usuario> AddAsync(Usuario usuario);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByIdAsync(int id);
    }
}
