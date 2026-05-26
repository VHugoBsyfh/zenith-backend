using Backend.DTOs.Admin;
using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IUsuarioAdminRepository
    {
        Task<(int total, List<UsuarioAdminListResponse> items)> ListarAsync(
            string? q, string? tipo, int page, int pageSize);
        Task<Usuario?> GetByIdAsync(int id);
        Task AtualizarTipoAsync(int id, string novoTipo);
        Task ResetarSenhaAsync(int id, string hash);
    }
}
