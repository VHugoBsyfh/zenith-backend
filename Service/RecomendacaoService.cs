using Backend.DTOs.Recomendacoes;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class RecomendacaoService
    {
        private readonly IRecomendacaoRepository _repo;
        public RecomendacaoService(IRecomendacaoRepository repo) => _repo = repo;

        public async Task<List<MissaoRecomendadaResponse>> RecomendarAsync(int idUsuario, int take = 10, string? localizacao = null)
        {
            // 1. Carrega dados do aventureiro logado
            var user = await _repo.ObterUsuarioAsync(idUsuario)
                       ?? throw new KeyNotFoundException("Usuário não encontrado.");

            // 2. Analisa afinidade por histórico de missões do usuário
            var afinidades = await _repo.ContarTiposConcluidosAsync(idUsuario);
            var maisFrequente = afinidades.Count == 0 ? null : afinidades.OrderByDescending(kv => kv.Value).First().Key;

            // 3. Descobre a capacidade de nível em equipe do usuário
            int maiorNivelGrupo = await _repo.ObterMaiorNivelEquipeAsync(idUsuario);

            // 4. Carrega o mural do banco
            var missoes = await _repo.ListarMissoesDisponiveisScoringAsync(idUsuario, localizacao);
            var resultados = new List<MissaoRecomendadaResponse>();

            foreach (var (m, criador) in missoes)
            {
                // TRAVA DE SEGURANÇA RPG:
                // Se o usuário está solo, barra missões acima do nível dele.
                if (maiorNivelGrupo == 0 && m.NivelMinimo > user.Nivel)
                    continue;

                // Se o usuário tem grupo, barra missões que superem até o membro mais forte do grupo.
                if (maiorNivelGrupo > 0 && m.NivelMinimo > maiorNivelGrupo)
                    continue;

                decimal score = 0;

                // REGRA 1: Classe preferida (Grande match individual)
                if (!string.IsNullOrWhiteSpace(m.ClassePreferida) &&
                    string.Equals(m.ClassePreferida, user.Classe, StringComparison.OrdinalIgnoreCase))
                {
                    score += 100;
                }

                // REGRA 2: Afinidade por tipo (Histórico)
                if (!string.IsNullOrWhiteSpace(maisFrequente) &&
                    string.Equals(m.TipoMissao, maisFrequente, StringComparison.OrdinalIgnoreCase))
                {
                    score += 30;
                }

                // REGRA 3: LÓGICA DE GRUPO VS SOLO (A MÁGICA)
                bool sugerirParaGrupo = false;

                if (m.NivelMinimo > user.Nivel && m.NivelMinimo <= maiorNivelGrupo)
                {
                    // A missão é muito forte para ele sozinho, mas o grupo dele aguenta!
                    score += 70; // Super bônus de incentivo de Party
                    sugerirParaGrupo = true;
                }
                else
                {
                    // Cálculo tradicional solo: quanto mais perto do nível do usuário, maior o score
                    var diff = Math.Abs(m.NivelMinimo - user.Nivel);
                    score += Math.Max(0, 40 - diff * 5);
                }

                // REGRA 4: Influência da recompensa em ouro (Logaritmo para balanceamento)
                var recompScore = (double)m.Recompensa <= 0 ? 0 : Math.Min(20m, (decimal)Math.Log10((double)m.Recompensa + 1) * 3);
                score += (decimal)recompScore;

                // Monta o objeto de resposta
                resultados.Add(new MissaoRecomendadaResponse
                {
                    Id = m.Id,
                    // Se for para grupo, injetamos uma tag no texto para o front-end renderizar uma estilização bonita
                    Titulo = sugerirParaGrupo ? $"[🏆 EQUIPE] {m.Titulo}" : m.Titulo,
                    Localizacao = m.Localizacao,
                    Recompensa = m.Recompensa,
                    TipoMissao = m.TipoMissao,
                    NivelMinimo = m.NivelMinimo,
                    ClassePreferida = m.ClassePreferida,
                    IdCriador = criador.Id,
                    NomeCriador = criador.Nome,
                    Score = Math.Round(score, 2)
                });
            }

            // Ordena pelo score calculado e entrega a quantidade solicitada (Take)
            return resultados
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.Recompensa)
                .Take(take <= 0 ? 10 : Math.Min(take, 50))
                .ToList();
        }
    }
}