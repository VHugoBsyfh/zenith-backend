using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IGrupoRepository
    {
        Task<Grupo> CriarAsync(Grupo grupo);
        Task<Grupo?> GetByIdAsync(int id);
        Task<int> CountMembrosAsync(int idGrupo);
        Task<bool> IsMembroAsync(int idGrupo, int idUsuario);
        Task AddMembroAsync(int idGrupo, int idUsuario);
        Task RemoveMembroAsync(int idGrupo, int idUsuario);
        Task<IEnumerable<Usuario>> ListarMembrosAsync(int idGrupo);
    }
}
