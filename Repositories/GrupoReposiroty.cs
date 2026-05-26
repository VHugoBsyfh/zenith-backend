using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class GrupoRepository : IGrupoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public GrupoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<Grupo> CriarAsync(Grupo grupo)
        {
            _ctx.Grupos.Add(grupo);
            await _ctx.SaveChangesAsync();
            return grupo;
        }

        public async Task<Grupo?> GetByIdAsync(int id)
            => await _ctx.Grupos.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

        public async Task<int> CountMembrosAsync(int idGrupo)
            => await _ctx.GrupoUsuarios.CountAsync(gu => gu.IdGrupo == idGrupo);

        public async Task<bool> IsMembroAsync(int idGrupo, int idUsuario)
            => await _ctx.GrupoUsuarios.AnyAsync(gu => gu.IdGrupo == idGrupo && gu.IdUsuario == idUsuario);

        public async Task AddMembroAsync(int idGrupo, int idUsuario)
        {
            _ctx.GrupoUsuarios.Add(new GrupoUsuario { IdGrupo = idGrupo, IdUsuario = idUsuario });
            await _ctx.SaveChangesAsync();
        }

        public async Task RemoveMembroAsync(int idGrupo, int idUsuario)
        {
            var ent = await _ctx.GrupoUsuarios.FirstOrDefaultAsync(gu => gu.IdGrupo == idGrupo && gu.IdUsuario == idUsuario);
            if (ent != null)
            {
                _ctx.GrupoUsuarios.Remove(ent);
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Usuario>> ListarMembrosAsync(int idGrupo)
        {
            var query = from gu in _ctx.GrupoUsuarios
                        join u in _ctx.Usuarios on gu.IdUsuario equals u.Id
                        where gu.IdGrupo == idGrupo
                        select u;

            return await query.AsNoTracking().ToListAsync();
        }
    }
}
