using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IRecomendacaoRepository
    {
        Task<Usuario?> ObterUsuarioAsync(int idUsuario);
        Task<Dictionary<string, int>> ContarTiposConcluidosAsync(int idUsuario, int topConsiderar = 3);
        Task<int> ObterMaiorNivelEquipeAsync(int idUsuario);
        Task<List<(Missao m, Usuario criador)>> ListarMissoesDisponiveisScoringAsync(int idUsuario, string? localizacao = null);
    }
}