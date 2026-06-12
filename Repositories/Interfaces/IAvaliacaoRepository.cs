using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IAvaliacaoRepository
    {
        Task<bool> MissaoEstaConcluidaAsync(int idMissao);
        Task<(bool participou, bool isSolo, int? idSolo, int? idGrupo)> VerificarParticipacaoAsync(int idMissaoAceita, int userId);
        Task<bool> JaAvaliouAsync(int idMissaoAceita, int avaliadorId, int avaliadoId);
        Task<Avaliacao> CriarAsync(Avaliacao a);
        Task<List<Avaliacao>> ListarPorMissaoAsync(int idMissaoAceita);
        Task<List<Avaliacao>> ListarRecebidasDoUsuarioAsync(int idUsuario);
        Task AtualizarHistoricoAvaliacaoAsync(int idUsuario, int idMissaoAceita, decimal nota, string? justificativa);
        Task<int> ObterNivelMissaoPorAceiteAsync(int idMissaoAceita);
        // No IAvaliacaoRepository ou IMissaoRepository
        Task<int?> ObterIdAceiteAsync(int idMissao, int idAvaliado);
        Task<int> ObterNivelMissaoAsync(int idMissao);
    }
}
