using Backend.DTOs.Common;
using Backend.DTOs.Penalidades;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class PenalidadeQueryService
    {
        private readonly IPenalidadeReadRepository _repo;
        public PenalidadeQueryService(IPenalidadeReadRepository repo) => _repo = repo;

        public async Task<PagedResponse<PenalidadeItemResponse>> ListarAsync(
            int idSolicitante, int idUsuarioAlvo,
            string? tipo, int? idMissaoAceita,
            DateTime? de, DateTime? ate,
            int page, int pageSize)
        {
            // Segurança: somente o próprio usuário pode ver
            if (idSolicitante != idUsuarioAlvo)
                throw new UnauthorizedAccessException("Você não pode ver penalidades de outro usuário.");

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
            var skip = (page - 1) * pageSize;

            var (total, items) = await _repo.ListarAsync(
                idUsuarioAlvo, tipo, idMissaoAceita, de, ate, skip, pageSize);

            return new PagedResponse<PenalidadeItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
