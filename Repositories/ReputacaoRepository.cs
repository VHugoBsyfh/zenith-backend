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

        public async Task<decimal> SomatorioPenalidadesAsync(int idUsuario)
        {
            return await _ctx.Penalidades
                .Where(p => p.IdUsuario == idUsuario && p.ValorPenalidade != null)
                .Select(p => p.ValorPenalidade!.Value)
                .DefaultIfEmpty(0m)
                .SumAsync();
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
