using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Missoes;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MissaoQueryRepository : Backend.Repositories.Interfaces.IMissaoQueryRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public MissaoQueryRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<PagedResponse<MissaoListItemResponse>> ListarAsync(MissaoFiltroRequest f)
        {
            var q =
                from m in _ctx.Missoes
                join u in _ctx.Usuarios on m.IdCriador equals u.Id
                select new { m, criador = u };

            if (!string.IsNullOrWhiteSpace(f.Status))
                q = q.Where(x => x.m.Status == f.Status);

            if (!string.IsNullOrWhiteSpace(f.Tipo))
                q = q.Where(x => x.m.TipoMissao == f.Tipo);

            if (!string.IsNullOrWhiteSpace(f.Localizacao))
                q = q.Where(x => x.m.Localizacao == f.Localizacao);

            if (!string.IsNullOrWhiteSpace(f.ClassePreferida))
                q = q.Where(x => x.m.ClassePreferida == f.ClassePreferida);

            if (f.RecompensaMin.HasValue)
                q = q.Where(x => x.m.Recompensa >= f.RecompensaMin.Value);

            if (f.RecompensaMax.HasValue)
                q = q.Where(x => x.m.Recompensa <= f.RecompensaMax.Value);

            if (f.NivelMaxUsuario.HasValue)
                q = q.Where(x => x.m.NivelMinimo <= f.NivelMaxUsuario.Value);

            // Ordenação
            var sort = (f.SortBy ?? "data").ToLowerInvariant();
            var dir = (f.SortDir ?? "desc").ToLowerInvariant();

            var ordered =
                sort switch
                {
                    "recompensa" => (dir == "asc")
                        ? q.OrderBy(x => x.m.Recompensa)
                        : q.OrderByDescending(x => x.m.Recompensa),

                    "nivel" => (dir == "asc")
                        ? q.OrderBy(x => x.m.NivelMinimo)
                        : q.OrderByDescending(x => x.m.NivelMinimo),

                    _ => (dir == "asc")
                        ? q.OrderBy(x => x.m.DataCriacao)
                        : q.OrderByDescending(x => x.m.DataCriacao)
                };

            // Paginação + projeção para DTO (continua igual)
            var page = f.Page <= 0 ? 1 : f.Page;
            var pageSize = f.PageSize <= 0 ? 20 : Math.Min(f.PageSize, 100);
            var skip = (page - 1) * pageSize;

            var total = await q.CountAsync();

            var items = await ordered
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new MissaoListItemResponse
                {
                    Id = x.m.Id,
                    Titulo = x.m.Titulo,
                    Localizacao = x.m.Localizacao,
                    Recompensa = x.m.Recompensa,
                    TipoMissao = x.m.TipoMissao,
                    NivelMinimo = x.m.NivelMinimo,
                    ClassePreferida = x.m.ClassePreferida,
                    Status = x.m.Status,
                    DataCriacao = x.m.DataCriacao,
                    IdCriador = x.criador.Id,
                    NomeCriador = x.criador.Nome
                })
                .AsNoTracking()
                .ToListAsync();

            return new PagedResponse<MissaoListItemResponse>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
