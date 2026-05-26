using Backend.Data;
using Backend.DTOs.Admin;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UsuarioAdminRepository : IUsuarioAdminRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public UsuarioAdminRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<(int total, List<UsuarioAdminListResponse> items)> ListarAsync(
            string? q, string? tipo, int page, int pageSize)
        {
            var query = _ctx.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(u => u.Nome.Contains(q) || u.Email.Contains(q));

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(u => u.TipoUsuario == tipo);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.DataCadastro)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UsuarioAdminListResponse
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Classe = u.Classe,
                    Nivel = u.Nivel,
                    TipoUsuario = u.TipoUsuario,
                    Reputacao = u.Reputacao,
                    DataCadastro = u.DataCadastro
                })
                .AsNoTracking()
                .ToListAsync();

            return (total, items);
        }

        public Task<Usuario?> GetByIdAsync(int id)
            => _ctx.Usuarios.FirstOrDefaultAsync(u => u.Id == id)!;

        public async Task AtualizarTipoAsync(int id, string novoTipo)
        {
            var u = await _ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == id)
                    ?? throw new KeyNotFoundException("Usuário não encontrado.");
            u.TipoUsuario = novoTipo;
            await _ctx.SaveChangesAsync();
        }

        public async Task ResetarSenhaAsync(int id, string hash)
        {
            var u = await _ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == id)
                    ?? throw new KeyNotFoundException("Usuário não encontrado.");
            u.Senha = hash;
            await _ctx.SaveChangesAsync();
        }
    }
}
