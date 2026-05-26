using Backend.DTOs.Common;
using Backend.DTOs.Missoes;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class MissaoQueryService
    {
        private readonly IMissaoQueryRepository _repo;
        public MissaoQueryService(IMissaoQueryRepository repo) => _repo = repo;

        public Task<PagedResponse<MissaoListItemResponse>> ListarAsync(MissaoFiltroRequest f)
            => _repo.ListarAsync(f);
    }
}
