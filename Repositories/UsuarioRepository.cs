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
    }
}
