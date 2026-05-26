using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AuthGuardRepository : IAuthGuardRepository
    {
        private readonly GuildaDigitalContext _ctx;

        public AuthGuardRepository(GuildaDigitalContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<bool> HasActiveBlockAsync(int idUsuario)
        {
            var now = DateTime.Now;

            return await _ctx.Penalidades.AnyAsync(p =>
                p.IdUsuario == idUsuario &&
                p.TipoPenalidade == "BloqueioLogin" &&
                p.DuracaoBloqueioDias.HasValue &&
                p.DuracaoBloqueioDias.Value > 0 &&
                p.DataAplicacao.AddDays(p.DuracaoBloqueioDias.Value) > now
            );
        }

        public async Task<int?> GetActiveBlockRemainingDaysAsync(int idUsuario)
        {
            var now = DateTime.Now;

            var pen = await _ctx.Penalidades
                .Where(p =>
                    p.IdUsuario == idUsuario &&
                    p.TipoPenalidade == "BloqueioLogin" &&
                    p.DuracaoBloqueioDias.HasValue &&
                    p.DuracaoBloqueioDias.Value > 0)
                .OrderByDescending(p => p.DataAplicacao)
                .FirstOrDefaultAsync();

            if (pen == null)
                return null;

            var diasPassados = (now - pen.DataAplicacao).Days;

            var restantes =
                pen.DuracaoBloqueioDias.Value - diasPassados;

            return restantes > 0 ? restantes : null;
        }

        public async Task ApplyBlockAsync(int idUsuario, int dias, string? motivo = null)
        {
            var p = new Penalidade
            {
                IdUsuario = idUsuario,
                IdMissaoAceita = 0,
                TipoPenalidade = "BloqueioLogin",
                ValorPenalidade = null,
                DuracaoBloqueioDias = Math.Max(1, dias),
                Justificativa = string.IsNullOrWhiteSpace(motivo)
                    ? "Bloqueio administrativo"
                    : motivo
            };

            _ctx.Penalidades.Add(p);

            await _ctx.SaveChangesAsync();
        }

        public async Task<bool> RemoveActiveBlockAsync(int idUsuario)
        {
            var now = DateTime.Now;

            var ativos = await _ctx.Penalidades
                .Where(p =>
                    p.IdUsuario == idUsuario &&
                    p.TipoPenalidade == "BloqueioLogin" &&
                    p.DuracaoBloqueioDias.HasValue &&
                    p.DuracaoBloqueioDias.Value > 0 &&
                    p.DataAplicacao.AddDays(p.DuracaoBloqueioDias.Value) > now
                )
                .ToListAsync();

            if (ativos.Count == 0)
                return false;

            foreach (var a in ativos)
            {
                a.DuracaoBloqueioDias = 0;
            }

            await _ctx.SaveChangesAsync();

            return true;
        }
    }
}