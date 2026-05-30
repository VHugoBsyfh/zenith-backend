using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IConclusaoRepository
    {
        Task<MissaoAceita?> GetAtivaAsync(int idMissaoAceita);
        Task AtualizarStatusAsync(MissaoAceita missao, string novoStatus);
        Task RegistrarHistoricoAsync(HistoricoMissao historico);
        Task AjustarReputacaoAsync(int idUsuario, decimal delta);
        Task<List<int>> ListarMembrosDoGrupoAsync(int idGrupo);
    }
}
