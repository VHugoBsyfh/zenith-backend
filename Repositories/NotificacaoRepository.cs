using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class NotificacaoRepository : INotificacaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public NotificacaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<Notificacao> CriarAsync(Notificacao n)
        {
            _ctx.Notificacoes.Add(n);
            await _ctx.SaveChangesAsync();
            return n;
        }

        public async Task<List<Notificacao>> ListarAsync(int idUsuario, bool somenteNaoLidas, int take, int skip)
        {
            var q = _ctx.Notificacoes
                .Where(n => n.IdUsuario == idUsuario);

            if (somenteNaoLidas)
                q = q.Where(n => !n.Lida);

            return await q
                .OrderByDescending(n => n.DataCriacao)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<int> ContarNaoLidasAsync(int idUsuario)
            => _ctx.Notificacoes.CountAsync(n => n.IdUsuario == idUsuario && !n.Lida);

        public Task<Notificacao?> GetByIdAsync(int id, int idUsuario)
            => _ctx.Notificacoes.FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario)!;

        public async Task MarcarComoLidaAsync(Notificacao n)
        {
            n.Lida = true;
            await _ctx.SaveChangesAsync();
        }
    }
}
