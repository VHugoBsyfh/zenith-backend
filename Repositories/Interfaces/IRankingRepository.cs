using Backend.DTOs.Rankings;

namespace Backend.Repositories.Interfaces
{
    public interface IRankingRepository
    {
        Task<List<RankingUsuarioItem>> GetTopAventureirosAsync(int take, int minMissoes);
        Task<List<RankingGrupoItem>> GetTopGruposAsync(int take, int minMissoes);
    }
}
