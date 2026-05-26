using Backend.DTOs.Historico;

namespace Backend.Repositories.Interfaces
{
    public interface IHistoricoRepository
    {
        Task<(int total, List<HistoricoItemResponse> items)> ListarHistoricoDoUsuarioAsync(
            int idUsuarioAlvo, string? resultado, DateTime? de, DateTime? ate, int skip, int take);

        Task<bool> UsuarioCriouMissaoAceitaAsync(int idSolicitante, int idMissaoAceita);
        Task<bool> UsuarioEhCriadorDaMissaoAsync(int idSolicitante, int idMissao);
    }
}
