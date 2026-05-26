using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Backend.Repositories.Interfaces;

namespace Backend.Repositories
{
    public class MissaoAceitaRepository : IMissaoAceitaRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public MissaoAceitaRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<bool> UsuarioTemAtivaAsync(int idUsuario)
            => await _ctx.MissoesAceitas.AnyAsync(a =>
                   a.IdUsuario == idUsuario &&
                   (a.StatusMissao == "Em andamento"));

        public async Task<bool> GrupoTemAtivaAsync(int idGrupo)
            => await _ctx.MissoesAceitas.AnyAsync(a =>
                   a.IdGrupo == idGrupo &&
                   (a.StatusMissao == "Em andamento"));

        public async Task<bool> MissaoJaFoiAceitaAsync(int idMissao)
            => await _ctx.MissoesAceitas.AnyAsync(a =>
                   a.IdMissao == idMissao &&
                   (a.StatusMissao == "Em andamento"));

        public async Task<MissaoAceita> CriarAsync(MissaoAceita registro)
        {
            _ctx.MissoesAceitas.Add(registro);
            await _ctx.SaveChangesAsync();
            return registro;
        }
        public async Task<bool> ExisteEmAndamentoParaUsuarioAsync(int idUsuario)
        {
            return await _ctx.MissoesAceitas
                .AnyAsync(ma => ma.IdUsuario == idUsuario && ma.StatusMissao == "Em andamento");
        }
        public async Task<bool> ExisteEmAndamentoParaAlgumMembroDoGrupoAsync(int idGrupo)
        {
            // se QUALQUER membro do grupo já tiver uma missão em andamento (solo OU via outro grupo), bloqueia
            var membrosIds = await _ctx.GrupoUsuarios
                .Where(g => g.IdGrupo == idGrupo)
                .Select(g => g.IdUsuario)
                .ToListAsync();

            if (!membrosIds.Any()) return false;

            return await _ctx.MissoesAceitas.AnyAsync(ma =>
                ma.StatusMissao == "Em andamento" &&
                (
                    (ma.IdUsuario != null && membrosIds.Contains(ma.IdUsuario.Value)) ||
                    (ma.IdGrupo != null && _ctx.GrupoUsuarios.Any(gu => gu.IdGrupo == ma.IdGrupo && membrosIds.Contains(gu.IdUsuario)))
                )
            );
        }
    }
}
