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
        //
        public async Task<DateTime?> ObterDataFimBloqueioAtivoAsync(int idUsuario)
        {
            // 1. Busca a última penalidade que tem dias de bloqueio
            var ultimoBloqueio = await _ctx.Penalidades
                .Where(p => p.IdUsuario == idUsuario && p.DuracaoBloqueioDias != null)
                .OrderByDescending(p => p.DataAplicacao)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (ultimoBloqueio == null)
                return null; // Nunca foi bloqueado

            // 2. A Matemática: Data que tomou o ban + Quantidade de dias
            var dataFimBloqueio = ultimoBloqueio.DataAplicacao.AddDays(ultimoBloqueio.DuracaoBloqueioDias.Value);

            // 3. Se a data do fim do bloqueio for MAIOR que o momento atual, ele ainda está bloqueado
            if (dataFimBloqueio > DateTime.Now)
            {
                return dataFimBloqueio; // Retorna até quando ele está bloqueado
            }

            // Se já passou do tempo, retorna null (está livre!)
            return null;
        }
    }
}
