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
            // 1. Traduzir IdMissao + IdAvaliado para IdMissaoAceita
            // Isso abstrai a complexidade do banco para o Front-end
            var idMissaoAceita = await _repo.ObterIdAceiteAsync(req.IdMissao, req.IdAvaliado)
                ?? throw new KeyNotFoundException("Não encontramos um aceite válido para esta missão e este usuário.");

            // 2. Validação do range de estrelas (1 a 5)
            if (req.Nota < 1 || req.Nota > 5)
                throw new InvalidOperationException("A nota de avaliação deve ser entre 1 e 5 estrelas.");

            // 3. Missão concluída? (Usando o ID traduzido)
            if (!await _repo.MissaoEstaConcluidaAsync(idMissaoAceita))
                throw new InvalidOperationException("A avaliação só é permitida após a missão ser concluída.");

            // 4. Participação do avaliador
            var (participou, isSolo, idSolo, idGrupo) = await _repo.VerificarParticipacaoAsync(idMissaoAceita, avaliadorId);
            if (!participou)
                throw new UnauthorizedAccessException("Você não participou desta missão.");

            if (avaliadorId == req.IdAvaliado)
                throw new InvalidOperationException("Não é permitido se autoavaliar.");

            // 5. Participação do avaliado
            var (participouAvaliado, _, idSoloAvaliado, idGrupoAvaliado) = await _repo.VerificarParticipacaoAsync(idMissaoAceita, req.IdAvaliado);
            if (!participouAvaliado)
                throw new InvalidOperationException("O usuário avaliado não participou desta missão.");

            // 6. Não pode avaliar alguém fora do mesmo contexto da aceitação (solo/grupo)
            if (isSolo != (idSoloAvaliado.HasValue))
                throw new InvalidOperationException("Avaliação inválida entre participantes de contextos diferentes.");

            // 7. Já avaliou esse alvo?
            if (await _repo.JaAvaliouAsync(idMissaoAceita, avaliadorId, req.IdAvaliado))
                throw new InvalidOperationException("Você já avaliou este participante nesta missão.");

            // 8. Criação da avaliação
            var ent = new Avaliacao
            {
                IdAvaliador = avaliadorId,
                IdAvaliado = req.IdAvaliado,
                IdMissaoAceita = idMissaoAceita, // Agora usando o ID correto traduzido
                Nota = req.Nota,
                Justificativa = string.IsNullOrWhiteSpace(req.Justificativa) ? null : req.Justificativa.Trim()
            };

            var saved = await _repo.CriarAsync(ent);

            // 9. Atualiza o histórico do avaliado
            await _repo.AtualizarHistoricoAvaliacaoAsync(req.IdAvaliado, idMissaoAceita, req.Nota, req.Justificativa);

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

        public async Task AvaliarGrupoAsync(int avaliadorId, AvaliacaoGrupoCreateRequest req, IGrupoRepository gruposRepo)
        {
            // 1. Validação de Estrelas
            if (req.Nota < 1 || req.Nota > 5)
                throw new InvalidOperationException("A nota de avaliação deve ser entre 1 e 5 estrelas.");

            // 2. Validação de Status (AGORA USA req.IdMissao)
            if (!await _repo.MissaoEstaConcluidaAsync(req.IdMissao))
                throw new InvalidOperationException("A avaliação só é permitida após a missão ser concluída.");

            // 3. Verifica participação (AGORA USA req.IdMissao)
            var (_, _, _, idGrupoParticipante) = await _repo.VerificarParticipacaoAsync(req.IdMissao, avaliadorId);
            if (idGrupoParticipante != req.IdGrupo)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para avaliar o grupo desta missão.");
            }

            var grupo = await gruposRepo.GetByIdAsync(req.IdGrupo)
                ?? throw new KeyNotFoundException("Grupo não encontrado.");

            // 4. Busca o nível (AGORA USA O MÉTODO DIRETO)
            int nivelMissao = await _repo.ObterNivelMissaoAsync(req.IdMissao);

            decimal variacaoReputacao = req.Nota switch
            {
                1 => -5m,
                2 => -2m,
                3 => 1m,
                4 => 3m,
                5 => 5m,
                _ => 0m
            };

            // Aplica o bônus de dificuldade para a Guilda se eles foram bem avaliados
            if (req.Nota >= 3)
            {
                variacaoReputacao += nivelMissao * 0.5m;
            }
            else if (req.Nota < 3)
            {
                // Punição por falhar em missão de alto nível
                variacaoReputacao -= (nivelMissao * 0.2m);
            }

            decimal novaReputacao = Math.Clamp(grupo.Reputacao + variacaoReputacao, 0m, 100m);
            await gruposRepo.AtualizarReputacaoAsync(req.IdGrupo, novaReputacao);
        }
    }
}
