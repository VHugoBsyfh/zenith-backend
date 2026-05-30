using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface ICancelamentoRepository
    {
        Task<MissaoAceita?> GetAtivaAsync(int idMissao);
        Task AtualizarParaCanceladaAsync(MissaoAceita reg, string motivo);
        Task RegistrarPenalidadeAsync(Penalidade p);
        Task<List<int>> ListarMembrosDoGrupoAsync(int idGrupo);
        Task AjustarReputacaoAsync(int idUsuario, decimal delta);
    }
}
