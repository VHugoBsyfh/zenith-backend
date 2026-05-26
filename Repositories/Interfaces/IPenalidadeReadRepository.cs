using Backend.DTOs.Penalidades;

namespace Backend.Repositories.Interfaces
{
    public interface IPenalidadeReadRepository
    {
        Task<(int total, List<PenalidadeItemResponse> items)> ListarAsync(
            int idUsuarioAlvo,
            string? tipo,
            int? idMissaoAceita,
            DateTime? de, DateTime? ate,
            int skip, int take);
    }
}
