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
    }
}