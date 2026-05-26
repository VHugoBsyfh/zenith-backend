using Backend.DTOs.Historico;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class HistoricoService
    {
        private readonly IHistoricoRepository _repo;
        public HistoricoService(IHistoricoRepository repo) => _repo = repo;

        public async Task<PagedResponse<HistoricoItemResponse>> ListarAsync(
            int idSolicitante, int idUsuarioAlvo, HistoricoFiltroRequest filtro, string? roleSolicitante)
        {
            // Segurança: pode ver o próprio histórico.
            // var podeVer = idSolicitante == idUsuarioAlvo;

            // Opcional: criador pode ver histórico de quem participou de suas missões — se quiser aplicar, mantenha só o próprio histórico por enquanto.
            // (Deixando a regra simples: apenas o próprio usuário)
            // if (!podeVer) throw new UnauthorizedAccessException("Você não pode ver o histórico de outro usuário.");

            var page = filtro.Page <= 0 ? 1 : filtro.Page;
            var pageSize = filtro.PageSize <= 0 ? 20 : Math.Min(filtro.PageSize, 100);
            var skip = (page - 1) * pageSize;

            var (total, items) = await _repo.ListarHistoricoDoUsuarioAsync(
                idUsuarioAlvo, filtro.Resultado, filtro.De, filtro.Ate, skip, pageSize);

            return new PagedResponse<HistoricoItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
