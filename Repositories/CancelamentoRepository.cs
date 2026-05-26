using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class CancelamentoRepository : ICancelamentoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public CancelamentoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<MissaoAceita?> GetAtivaAsync(int idMissaoAceita)
            => await _ctx.MissoesAceitas.FirstOrDefaultAsync(x => x.Id == idMissaoAceita && x.StatusMissao == "Em andamento");

        public async Task AtualizarParaCanceladaAsync(MissaoAceita reg, string motivo)
        {
            reg.StatusMissao = "Cancelada";
            reg.MotivoCancelamento = motivo;
            reg.DataConclusao = DateTime.Now;

            var missao = await _ctx.Missoes.FirstOrDefaultAsync(m => m.Id == reg.IdMissao);
            if (missao != null) missao.Status = "Cancelada";

            await _ctx.SaveChangesAsync();
        }

        public async Task RegistrarPenalidadeAsync(Penalidade p)
        {
            _ctx.Penalidades.Add(p);
            await _ctx.SaveChangesAsync();
        }

        public async Task<List<int>> ListarMembrosDoGrupoAsync(int idGrupo)
            => await _ctx.GrupoUsuarios
                         .Where(g => g.IdGrupo == idGrupo)
                         .Select(g => g.IdUsuario)
                         .ToListAsync();

        public async Task AjustarReputacaoAsync(int idUsuario, decimal delta)
        {
            var u = await _ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == idUsuario);
            if (u == null) return;

            var nova = Math.Max(0m, u.Reputacao + delta);
            u.Reputacao = nova;
            await _ctx.SaveChangesAsync();
        }

    }
}
