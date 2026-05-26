using Backend.Data;
using Backend.DTOs.Penalidades;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PenalidadeReadRepository : IPenalidadeReadRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public PenalidadeReadRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<(int total, List<PenalidadeItemResponse> items)> ListarAsync(
            int idUsuarioAlvo,
            string? tipo,
            int? idMissaoAceita,
            DateTime? de, DateTime? ate,
            int skip, int take)
        {
            var q = _ctx.Penalidades
                .Where(p => p.IdUsuario == idUsuarioAlvo)
                .Select(p => new PenalidadeItemResponse
                {
                    Id = p.Id,
                    IdUsuario = p.IdUsuario,
                    IdMissaoAceita = p.IdMissaoAceita,
                    TipoPenalidade = p.TipoPenalidade,
                    ValorPenalidade = p.ValorPenalidade,
                    DuracaoBloqueioDias = p.DuracaoBloqueioDias,
                    Justificativa = p.Justificativa,
                    DataAplicacao = p.DataAplicacao
                })
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                q = q.Where(x => x.TipoPenalidade == tipo);

            if (idMissaoAceita.HasValue)
                q = q.Where(x => x.IdMissaoAceita == idMissaoAceita.Value);

            if (de.HasValue)
                q = q.Where(x => x.DataAplicacao >= de.Value);

            if (ate.HasValue)
                q = q.Where(x => x.DataAplicacao <= ate.Value);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(x => x.DataAplicacao)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();

            return (total, items);
        }
    }
}
