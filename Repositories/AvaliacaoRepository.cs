using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AvaliacaoRepository : IAvaliacaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public AvaliacaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<bool> MissaoEstaConcluidaAsync(int idMissaoAceita)
            => await _ctx.MissoesAceitas.AnyAsync(m => m.Id == idMissaoAceita && m.StatusMissao == "Concluída");

        public async Task<(bool participou, bool isSolo, int? idSolo, int? idGrupo)> VerificarParticipacaoAsync(int idMissaoAceita, int userId)
        {
            var reg = await _ctx.MissoesAceitas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == idMissaoAceita);
            if (reg == null) return (false, false, null, null);

            if (reg.IdUsuario.HasValue) // solo
                return (reg.IdUsuario.Value == userId, true, reg.IdUsuario.Value, null);

            if (reg.IdGrupo.HasValue)
            {
                var isMember = await _ctx.GrupoUsuarios.AnyAsync(gu => gu.IdGrupo == reg.IdGrupo && gu.IdUsuario == userId);
                return (isMember, false, null, reg.IdGrupo);
            }

            return (false, false, null, null);
        }

        public async Task<bool> JaAvaliouAsync(int idMissaoAceita, int avaliadorId, int avaliadoId)
            => await _ctx.Avaliacoes.AnyAsync(a =>
                  a.IdMissaoAceita == idMissaoAceita &&
                  a.IdAvaliador == avaliadorId &&
                  a.IdAvaliado == avaliadoId);

        public async Task<Avaliacao> CriarAsync(Avaliacao a)
        {
            _ctx.Avaliacoes.Add(a);
            await _ctx.SaveChangesAsync();
            return a;
        }

        public async Task<List<Avaliacao>> ListarPorMissaoAsync(int idMissaoAceita)
            => await _ctx.Avaliacoes.AsNoTracking()
                   .Where(a => a.IdMissaoAceita == idMissaoAceita)
                   .OrderByDescending(a => a.DataAvaliacao)
                   .ToListAsync();

        public async Task<List<Avaliacao>> ListarRecebidasDoUsuarioAsync(int idUsuario)
            => await _ctx.Avaliacoes.AsNoTracking()
                   .Where(a => a.IdAvaliado == idUsuario)
                   .OrderByDescending(a => a.DataAvaliacao)
                   .ToListAsync();

        public async Task AtualizarHistoricoAvaliacaoAsync(int idUsuario, int idMissaoAceita, decimal nota, string? justificativa)
        {
            // Atualiza/insere nota no HistoricoMissoes (entrada do avaliado)
            var h = await _ctx.HistoricoMissoes
                     .FirstOrDefaultAsync(x => x.IdUsuario == idUsuario && x.IdMissaoAceita == idMissaoAceita);

            if (h != null)
            {
                h.AvaliacaoRecebida = nota;
                if (!string.IsNullOrWhiteSpace(justificativa))
                    h.Justificativa = justificativa;
                await _ctx.SaveChangesAsync();
            }
            // se não existir histórico, poderíamos criar — mas, via fluxo, ele é criado na conclusão/cancelamento
        }
    }
}
