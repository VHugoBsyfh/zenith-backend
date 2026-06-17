using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Backend.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public UsuarioRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<bool> EmailExisteAsync(string email)
            => await _ctx.Usuarios.AnyAsync(u => u.Email == email);

        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            _ctx.Usuarios.Add(usuario);
            await _ctx.SaveChangesAsync();
            return usuario;
        }
        public async Task<Usuario?> GetByIdAsync(int id) => await _ctx.Usuarios.FindAsync(id);

        public async Task<Usuario?> GetByEmailAsync(string email)
           => await _ctx.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        public async Task AtualizarNivelAsync(int idUsuario, int novoNivel)
        {
            var u = await _ctx.Usuarios.FindAsync(idUsuario);
            if (u != null)
            {
                u.Nivel = novoNivel;
                await _ctx.SaveChangesAsync();
            }
        }
        public async Task<List<Usuario>> ListarAsync(string? role, int? id)
        {
            var query = _ctx.Usuarios.AsQueryable();

            // 1. Filtro por ID (se foi passado, só vai trazer ele)
            if (id.HasValue && id.Value > 0)
            {
                query = query.Where(u => u.Id == id.Value);
            }

            // 2. Filtro por Role (mantemos a que funcionou pra você)
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.TipoUsuario == role);
            }

            return await query.ToListAsync();
        }
        //
        public async Task<decimal> ObterValorAcumuladoAsync(int idUsuario)
        {
            // 1. Soma o valor das missões Solo concluídas por ele
            var totalSolo = await _ctx.Missoes
                .Where(m => m.IdAventureiro == idUsuario && m.Status == "Concluída")
                .SumAsync(m => m.Recompensa);

            // 2. Acha os grupos que esse usuário faz parte
            var idsGrupos = await _ctx.GrupoUsuarios
                .Where(gu => gu.IdUsuario == idUsuario)
                .Select(gu => gu.IdGrupo)
                .ToListAsync();

            // 3. Soma o valor das missões em Grupo concluídas pelas guildas dele
            var totalGrupo = await _ctx.Missoes
                .Where(m => m.IdGrupo != null && idsGrupos.Contains(m.IdGrupo.Value) && m.Status == "Concluída")
                .SumAsync(m => m.Recompensa);

            // 4. Retorna a soma de tudo!
            return totalSolo + totalGrupo;
        }
    }
}
