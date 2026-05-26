using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ContratoRepository : IContratoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public ContratoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public Task<MissaoAceita?> GetAceitacaoAsync(int idMissaoAceita)
            => _ctx.MissoesAceitas.FirstOrDefaultAsync(x => x.Id == idMissaoAceita)!;

        public Task<Missao?> GetMissaoAsync(int idMissao)
            => _ctx.Missoes.FirstOrDefaultAsync(x => x.Id == idMissao)!;

        public Task<List<int>> ListarMembrosDoGrupoAsync(int idGrupo)
            => _ctx.GrupoUsuarios.Where(g => g.IdGrupo == idGrupo).Select(g => g.IdUsuario).ToListAsync();

        public Task<Usuario?> GetUsuarioAsync(int id)
            => _ctx.Usuarios.FirstOrDefaultAsync(x => x.Id == id)!;

        public Task<Contrato?> GetByMissaoAceitaAsync(int idMissaoAceita)
            => _ctx.Contratos.FirstOrDefaultAsync(c => c.IdMissaoAceita == idMissaoAceita)!;

        public async Task<Contrato> CreateAsync(Contrato c)
        {
            _ctx.Contratos.Add(c);
            await _ctx.SaveChangesAsync();
            return c;
        }

        public Task<Contrato?> GetByIdAsync(int idContrato)
            => _ctx.Contratos.FirstOrDefaultAsync(c => c.Id == idContrato)!;

        public Task SaveAsync() => _ctx.SaveChangesAsync();
    }
}
