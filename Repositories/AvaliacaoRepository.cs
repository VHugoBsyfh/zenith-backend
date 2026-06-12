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

        public async Task<int> ObterNivelMissaoAsync(int idMissao)
        {
            var missao = await _ctx.Missoes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == idMissao);
            return missao?.NivelMinimo ?? 1;
        }
        public async Task<bool> MissaoEstaConcluidaAsync(int idMissao)
        {
            // Agora olhamos direto na tabela principal de Missoes
            var missao = await _ctx.Missoes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == idMissao);

            if (missao == null || string.IsNullOrWhiteSpace(missao.Status))
                return false;

            string status = missao.Status.Trim();

            return status.Equals("Concluida", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Concluída", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<(bool participou, bool isSolo, int? idSolo, int? idGrupo)> VerificarParticipacaoAsync(int idMissao, int userId)
        {
            // Busca direto a missão principal
            var missao = await _ctx.Missoes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == idMissao);
            if (missao == null) return (false, false, null, null);

            bool isCriador = missao.IdCriador == userId;

            if (missao.IdAventureiro.HasValue) // Fluxo Solo
            {
                bool participou = (missao.IdAventureiro.Value == userId) || isCriador;
                return (participou, true, missao.IdAventureiro.Value, null);
            }

            if (missao.IdGrupo.HasValue) // Fluxo Grupo
            {
                var isMember = await _ctx.GrupoUsuarios.AnyAsync(gu => gu.IdGrupo == missao.IdGrupo.Value && gu.IdUsuario == userId);
                bool participou = isMember || isCriador;
                return (participou, false, null, missao.IdGrupo.Value);
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
        public async Task<int> ObterNivelMissaoPorAceiteAsync(int idMissaoAceita)
        {
            return await (from ma in _ctx.MissoesAceitas
                          join m in _ctx.Missoes on ma.IdMissao equals m.Id
                          where ma.Id == idMissaoAceita
                          select m.NivelMinimo).FirstOrDefaultAsync();
        }
        //
        public async Task<int?> ObterIdAceiteAsync(int idMissao, int idAvaliado)
        {
            var aceite = await _ctx.MissoesAceitas
                .FirstOrDefaultAsync(m => m.IdMissao == idMissao && m.IdUsuario == idAvaliado);

            return aceite?.Id;
        }
    }
}
