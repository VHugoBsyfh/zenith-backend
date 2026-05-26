using Backend.DTOs.Recomendacoes;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class RecomendacaoService
    {
        private readonly IRecomendacaoRepository _repo;
        public RecomendacaoService(IRecomendacaoRepository repo) => _repo = repo;

        public async Task<List<MissaoRecomendadaResponse>> RecomendarAsync(
            int idUsuario, int take = 10, string? localizacao = null)
        {
            var user = await _repo.ObterUsuarioAsync(idUsuario)
                       ?? throw new KeyNotFoundException("Usuário não encontrado.");

            var afinidades = await _repo.ContarTiposConcluidosAsync(idUsuario); // ex.: { "Combate": 7, "Diplomacia": 3 }
            var maisFrequente = afinidades.Count == 0 ? null : afinidades.OrderByDescending(kv => kv.Value).First().Key;

            var missoes = await _repo.ListarMissoesDisponiveisAsync(localizacao);

            // Scoring simples, transparente e eficiente
            var resultados = new List<MissaoRecomendadaResponse>();

            foreach (var (m, criador) in missoes)
            {
                decimal score = 0;

                // 1) Classe preferida
                if (!string.IsNullOrWhiteSpace(m.ClassePreferida) &&
                    string.Equals(m.ClassePreferida, user.Classe, StringComparison.OrdinalIgnoreCase))
                {
                    score += 50; // grande bônus
                }

                // 2) Afinidade por tipo (histórico do usuário)
                if (!string.IsNullOrWhiteSpace(maisFrequente) &&
                    string.Equals(m.TipoMissao, maisFrequente, StringComparison.OrdinalIgnoreCase))
                {
                    score += 30;
                }

                // 3) Compatibilidade de nível
                var diff = Math.Abs(m.NivelMinimo - user.Nivel);
                var nivelScore = Math.Max(0, 40 - diff * 5); // até 40, decai 5 por nível de distância
                score += nivelScore;

                // 4) Recompensa leve influência (+0..10)
                var recompScore = (double)m.Recompensa <= 0 ? 0 : Math.Min(10m, (decimal)Math.Log10((double)m.Recompensa + 1));
                score += (decimal)recompScore;

                resultados.Add(new MissaoRecomendadaResponse
                {
                    Id = m.Id,
                    Titulo = m.Titulo,
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

            return resultados
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.Recompensa)
                .ThenBy(r => r.NivelMinimo)
                .Take(take <= 0 ? 10 : Math.Min(take, 50))
                .ToList();
        }
    }
}
