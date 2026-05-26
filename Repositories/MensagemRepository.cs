using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MensagemRepository : IMensagemRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public MensagemRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<Mensagem> EnviarAsync(Mensagem msg)
        {
            _ctx.Mensagens.Add(msg);
            await _ctx.SaveChangesAsync();
            return msg;
        }

        public async Task<List<(Mensagem msg, string autorNome)>> ListarPorGrupoAsync(int idGrupo, int skip, int take)
        {
            var query =
                from m in _ctx.Mensagens
                join u in _ctx.Usuarios on m.IdUsuario equals u.Id
                where m.IdGrupo == idGrupo
                orderby m.DataEnvio descending
                select new { m, u.Nome };

            var list = await query.Skip(skip).Take(take).AsNoTracking().ToListAsync();
            return list.Select(x => ((Mensagem)x.m, x.Nome)).ToList();
        }
    }
}
