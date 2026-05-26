using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IContratoRepository
    {
        Task<MissaoAceita?> GetAceitacaoAsync(int idMissaoAceita);
        Task<Missao?> GetMissaoAsync(int idMissao);
        Task<List<int>> ListarMembrosDoGrupoAsync(int idGrupo);
        Task<Usuario?> GetUsuarioAsync(int id);

        Task<Contrato?> GetByMissaoAceitaAsync(int idMissaoAceita);
        Task<Contrato> CreateAsync(Contrato c);
        Task<Contrato?> GetByIdAsync(int idContrato);
        Task SaveAsync();
    }
}
