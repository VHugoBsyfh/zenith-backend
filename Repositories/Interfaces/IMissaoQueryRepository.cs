using Backend.DTOs.Common;
using Backend.DTOs.Missoes;

namespace Backend.Repositories.Interfaces
{
    public interface IMissaoQueryRepository
    {
        Task<PagedResponse<MissaoListItemResponse>> ListarAsync(MissaoFiltroRequest f);
    }
}
