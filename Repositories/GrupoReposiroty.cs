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
    => await _ctx.Usuarios.CountAsync(u => u.IdGrupo == idGrupo);

        public async Task<bool> IsMembroAsync(int idGrupo, int idUsuario)
    => await _ctx.Usuarios.AnyAsync(u => u.IdGrupo == idGrupo && u.Id == idUsuario);

        public async Task AddMembroAsync(int idGrupo, int idUsuario)
        {
            var usuario = await _ctx.Usuarios.FindAsync(idUsuario);
            if (usuario != null)
            {
                usuario.IdGrupo = idGrupo; // Atualiza a coluna direto no usuário
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task RemoveMembroAsync(int idGrupo, int idUsuario)
        {
            var usuario = await _ctx.Usuarios.FindAsync(idUsuario);
            if (usuario != null)
            {
                usuario.IdGrupo = null; // Tira ele do grupo
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Usuario>> ListarMembrosAsync(int idGrupo)
        {
            // Olha como a consulta ficou infinitamente mais simples sem o JOIN!
            return await _ctx.Usuarios
                .Where(u => u.IdGrupo == idGrupo)
                .AsNoTracking()
                .ToListAsync();
        }
        //
        // ▼ NOVA IMPLEMENTAÇÃO ▼
        public async Task DeletarGrupoAsync(int idGrupo)
        {
            // Busca o grupo no banco
            var grupo = await _ctx.Grupos.FindAsync(idGrupo);

            if (grupo != null)
            {
                // Remove e salva
                _ctx.Grupos.Remove(grupo);
                await _ctx.SaveChangesAsync();
            }
        }
        //
        public async Task AtualizarReputacaoAsync(int idGrupo, decimal novaReputacao)
        {
            var grupo = await _ctx.Grupos.FirstOrDefaultAsync(g => g.Id == idGrupo);
            if (grupo != null)
            {
                grupo.Reputacao = novaReputacao;
                await _ctx.SaveChangesAsync();
            }
        }
        //
        public async Task<List<Grupo>> ListarAsync(int? id)
        {
            var query = _ctx.Grupos.AsQueryable();

            if (id.HasValue && id.Value > 0)
            {
                query = query.Where(g => g.Id == id.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }
    }
}
