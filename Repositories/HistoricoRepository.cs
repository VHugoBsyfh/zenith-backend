using Backend.Data;
using Backend.DTOs.Historico;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class HistoricoRepository : Backend.Repositories.Interfaces.IHistoricoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public HistoricoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<(int total, List<HistoricoItemResponse> items)> ListarHistoricoDoUsuarioAsync(
            int idUsuarioAlvo, string? resultado, DateTime? de, DateTime? ate, int skip, int take)
        {
            var baseQuery =
                from h in _ctx.HistoricoMissoes
                join ma in _ctx.MissoesAceitas on h.IdMissaoAceita equals ma.Id
                join m in _ctx.Missoes on ma.IdMissao equals m.Id
                join uCriador in _ctx.Usuarios on m.IdCriador equals uCriador.Id
                where h.IdUsuario == idUsuarioAlvo
                select new HistoricoItemResponse
                {
                    IdHistorico = h.Id,
                    IdMissaoAceita = h.IdMissaoAceita,
                    IdMissao = m.Id,
                    TituloMissao = m.Titulo,
                    Resultado = h.Resultado,
                    AvaliacaoRecebida = h.AvaliacaoRecebida,
                    Justificativa = h.Justificativa,
                    DataRegistro = h.DataRegistro,
                    IdCriador = uCriador.Id,
                    NomeCriador = uCriador.Nome
                };

            if (!string.IsNullOrWhiteSpace(resultado))
                baseQuery = baseQuery.Where(x => x.Resultado == resultado);

            if (de.HasValue)  baseQuery = baseQuery.Where(x => x.DataRegistro >= de.Value);
            if (ate.HasValue) baseQuery = baseQuery.Where(x => x.DataRegistro <= ate.Value);

            var total = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(x => x.DataRegistro)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();

            return (total, items);
        }

        public async Task<bool> UsuarioCriouMissaoAceitaAsync(int idSolicitante, int idMissaoAceita)
        {
            var ma = await _ctx.MissoesAceitas.FirstOrDefaultAsync(x => x.Id == idMissaoAceita);
            if (ma == null) return false;
            var m = await _ctx.Missoes.FirstOrDefaultAsync(x => x.Id == ma.IdMissao);
            return m != null && m.IdCriador == idSolicitante;
        }

        public async Task<bool> UsuarioEhCriadorDaMissaoAsync(int idSolicitante, int idMissao)
        {
            var m = await _ctx.Missoes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == idMissao);
            return m != null && m.IdCriador == idSolicitante;
        }
    }
}
