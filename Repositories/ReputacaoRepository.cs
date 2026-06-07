using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ReputacaoRepository : IReputacaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public ReputacaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<List<Avaliacao>> ListarAvaliacoesRecebidasAsync(int idUsuario, int max = 50)
        {
            return await _ctx.Avaliacoes
                .Where(a => a.IdAvaliado == idUsuario)
                .OrderByDescending(a => a.DataAvaliacao)
                .Take(max)
                .AsNoTracking()
                .ToListAsync();
        }

        // ▼ MÉTODO CORRIGIDO ▼
        public async Task<decimal> SomatorioPenalidadesAsync(int idUsuario)
        {
            // O SumAsync em um tipo anulável retorna um decimal? (nulo se não houver registros)
            var total = await _ctx.Penalidades
                .Where(p => p.IdUsuario == idUsuario)
                .SumAsync(p => p.ValorPenalidade);

            // Retorna o valor somado ou 0m caso venha nulo do banco
            return total ?? 0m;
        }
        //
        public async Task<int> SomatorioDiasBloqueioAsync(int idUsuario)
        {
            var totalDias = await _ctx.Penalidades
                .Where(p => p.IdUsuario == idUsuario)
                .SumAsync(p => p.DuracaoBloqueioDias); // Usa a propriedade exata do seu Model

            return totalDias ?? 0;
        }

        public async Task AtualizarReputacaoAsync(int idUsuario, decimal reputacao)
        {
            var u = await _ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == idUsuario);
            if (u == null) return;
            u.Reputacao = reputacao;
            await _ctx.SaveChangesAsync();
        }
        public async Task<int> ContarMissoesConcluidasAsync(int idUsuario)
        {
            return await _ctx.HistoricoMissoes
                .CountAsync(h => h.IdUsuario == idUsuario && h.Resultado == "Concluída");
        }
        //
        public async Task<List<(Avaliacao avaliacao, int nivelMinimo)>> ListarAvaliacoesComNivelAsync(int idUsuario, int max = 50)
        {
            var query = from a in _ctx.Avaliacoes
                        join ma in _ctx.MissoesAceitas on a.IdMissaoAceita equals ma.Id
                        join m in _ctx.Missoes on ma.IdMissao equals m.Id
                        where a.IdAvaliado == idUsuario
                        orderby a.DataAvaliacao descending
                        select new { a, m.NivelMinimo };

            var list = await query.Take(max).AsNoTracking().ToListAsync();
            return list.Select(x => (x.a, x.NivelMinimo)).ToList();
        }
    }
}