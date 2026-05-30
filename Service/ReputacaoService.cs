using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ReputacaoService
    {
        private readonly IReputacaoRepository _repo;
        public ReputacaoService(IReputacaoRepository repo) => _repo = repo;

        public async Task<(decimal Reputacao, int BloqueioDias)> RecalcularAsync(int idUsuario)
        {
            var avals = await _repo.ListarAvaliacoesRecebidasAsync(idUsuario, max: 50);
            
            var totalPen = await _repo.SomatorioPenalidadesAsync(idUsuario);
            var totalDiasBloqueio = await _repo.SomatorioDiasBloqueioAsync(idUsuario);
            
            // ▼ 1. BUSCA O TOTAL DE MISSÕES CONCLUÍDAS ▼
            var totalConcluidas = await _repo.ContarMissoesConcluidasAsync(idUsuario);
            decimal bonusConclusao = totalConcluidas * 10m;

            // Cenário A: Utilizador ainda não tem avaliações dos contratantes
            if (avals.Count == 0)
            {
                // Começa com 100 de reputação base, retira as penalidades e adiciona o bónus
                var rep = Clamp(100m - totalPen + bonusConclusao, 0m, 100m);
                await _repo.AtualizarReputacaoAsync(idUsuario, rep);
                return (rep, totalDiasBloqueio);
            }

            // Cenário B: Utilizador já possui avaliações
            decimal somaPesos = 0m;
            decimal soma = 0m;
            decimal peso = 1.0m;
            
            foreach (var a in avals)
            {
                soma += a.Nota * peso;
                somaPesos += peso;
                peso = Math.Max(0.5m, peso - 0.02m);
            }
            
            var media = somaPesos > 0 ? soma / somaPesos : 100m;
            
            // ▼ 2. APLICA O BÓNUS DE CONCLUSÃO NA FÓRMULA FINAL ▼
            var reputacao = Clamp(media - totalPen + bonusConclusao, 0m, 100m);

            await _repo.AtualizarReputacaoAsync(idUsuario, reputacao);
            
            return (reputacao, totalDiasBloqueio);
        }

        private static decimal Clamp(decimal v, decimal min, decimal max)
            => v < min ? min : (v > max ? max : v);
    }
}