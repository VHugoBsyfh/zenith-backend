using Backend.DTOs.Rankings;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class RankingService
    {
        private readonly IRankingRepository _repo;
        public RankingService(IRankingRepository repo) => _repo = repo;

        // Lembre-se de atualizar a Interface IRankingRepository também!
        public Task<List<RankingUsuarioItem>> TopAventureirosAsync(int take, int minMissoes, string? orderBy)
            => _repo.GetTopAventureirosAsync(NormalizeTake(take), Math.Max(0, minMissoes), orderBy);

        public Task<List<RankingGrupoItem>> TopGruposAsync(int take, int minMissoes)
            => _repo.GetTopGruposAsync(NormalizeTake(take), Math.Max(0, minMissoes));

        private static int NormalizeTake(int take)
            => take <= 0 ? 10 : Math.Min(take, 100);
    }
}
