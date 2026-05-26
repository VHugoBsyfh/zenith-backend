using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ConclusaoRepository : IConclusaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public ConclusaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<MissaoAceita?> GetAtivaAsync(int idMissaoAceita)
            => await _ctx.MissoesAceitas.FirstOrDefaultAsync(x => x.Id == idMissaoAceita && x.StatusMissao == "Em andamento");

        public async Task AtualizarStatusAsync(MissaoAceita missao, string novoStatus)
        {
            missao.StatusMissao = novoStatus;
            missao.DataConclusao = DateTime.Now;
            await _ctx.SaveChangesAsync();
        }

        public async Task RegistrarHistoricoAsync(HistoricoMissao historico)
        {
            _ctx.HistoricoMissoes.Add(historico);
            await _ctx.SaveChangesAsync();
        }
    }
}
