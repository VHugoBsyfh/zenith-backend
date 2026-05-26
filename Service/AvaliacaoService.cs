using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class AvaliacaoService
    {
        private readonly IAvaliacaoRepository _repo;
        private readonly ReputacaoService _reputacaoService;
        public AvaliacaoService(IAvaliacaoRepository repo, ReputacaoService reputacaoService)
        {
            _repo = repo;
            _reputacaoService = reputacaoService;
        }

        public AvaliacaoService(IAvaliacaoRepository repo) => _repo = repo;

        public async Task<AvaliacaoResponse> CriarAsync(int avaliadorId, AvaliacaoCreateRequest req)
        {
            // Missão concluída?
            if (!await _repo.MissaoEstaConcluidaAsync(req.IdMissaoAceita))
                throw new InvalidOperationException("A avaliação só é permitida após a missão ser concluída.");

            // Participação do avaliador
            var (participou, isSolo, idSolo, idGrupo) = await _repo.VerificarParticipacaoAsync(req.IdMissaoAceita, avaliadorId);
            if (!participou)
                throw new UnauthorizedAccessException("Você não participou desta missão.");

            if (avaliadorId == req.IdAvaliado)
                throw new InvalidOperationException("Não é permitido se autoavaliar.");

            // Participação do avaliado
            var (participouAvaliado, _, idSoloAvaliado, idGrupoAvaliado) = await _repo.VerificarParticipacaoAsync(req.IdMissaoAceita, req.IdAvaliado);
            if (!participouAvaliado)
                throw new InvalidOperationException("O usuário avaliado não participou desta missão.");

            // Não pode avaliar alguém fora do mesmo contexto da aceitação (solo/grupo)
            if (isSolo != (idSoloAvaliado.HasValue))
                throw new InvalidOperationException("Avaliação inválida entre participantes de contextos diferentes.");

            // Já avaliou esse alvo?
            if (await _repo.JaAvaliouAsync(req.IdMissaoAceita, avaliadorId, req.IdAvaliado))
                throw new InvalidOperationException("Você já avaliou este participante nesta missão.");

            var ent = new Avaliacao
            {
                IdAvaliador = avaliadorId,
                IdAvaliado = req.IdAvaliado,
                IdMissaoAceita = req.IdMissaoAceita,
                Nota = req.Nota,
                Justificativa = string.IsNullOrWhiteSpace(req.Justificativa) ? null : req.Justificativa.Trim()
            };

            var saved = await _repo.CriarAsync(ent);

            // Atualiza o histórico do avaliado (campo AvaliacaoRecebida)
            await _repo.AtualizarHistoricoAvaliacaoAsync(req.IdAvaliado, req.IdMissaoAceita, req.Nota, req.Justificativa);
            await _reputacaoService.RecalcularAsync(req.IdAvaliado);

            return new AvaliacaoResponse
            {
                Id = saved.Id,
                IdAvaliador = saved.IdAvaliador,
                IdAvaliado = saved.IdAvaliado,
                IdMissaoAceita = saved.IdMissaoAceita,
                Nota = saved.Nota,
                Justificativa = saved.Justificativa,
                DataAvaliacao = saved.DataAvaliacao
            };
        }

        public async Task<IEnumerable<AvaliacaoResponse>> ListarPorMissaoAsync(int idMissaoAceita)
            => (await _repo.ListarPorMissaoAsync(idMissaoAceita))
               .Select(a => new AvaliacaoResponse
               {
                   Id = a.Id,
                   IdAvaliador = a.IdAvaliador,
                   IdAvaliado = a.IdAvaliado,
                   IdMissaoAceita = a.IdMissaoAceita,
                   Nota = a.Nota,
                   Justificativa = a.Justificativa,
                   DataAvaliacao = a.DataAvaliacao
               });

        public async Task<IEnumerable<AvaliacaoResponse>> ListarRecebidasDoUsuarioAsync(int idUsuario)
            => (await _repo.ListarRecebidasDoUsuarioAsync(idUsuario))
               .Select(a => new AvaliacaoResponse
               {
                   Id = a.Id,
                   IdAvaliador = a.IdAvaliador,
                   IdAvaliado = a.IdAvaliado,
                   IdMissaoAceita = a.IdMissaoAceita,
                   Nota = a.Nota,
                   Justificativa = a.Justificativa,
                   DataAvaliacao = a.DataAvaliacao
               });
    }
}
