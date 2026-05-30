using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ReputacaoService
    {
        private readonly IReputacaoRepository _repo;
        public ReputacaoService(IReputacaoRepository repo) => _repo = repo;

        public async Task<(decimal Reputacao, int BloqueioDias)> RecalcularAsync(int idUsuario)
        {
            // Define o valor padrão (ex: 50)
            const decimal valorBaseDefault = 50m;

            var avals = await _repo.ListarAvaliacoesRecebidasAsync(idUsuario, max: 50);
            var totalPen = await _repo.SomatorioPenalidadesAsync(idUsuario);
            var totalDiasBloqueio = await _repo.SomatorioDiasBloqueioAsync(idUsuario);
            var totalConcluidas = await _repo.ContarMissoesConcluidasAsync(idUsuario);

            decimal bonusConclusao = totalConcluidas * 10m;

            if (avals.Count == 0)
            {
                // Usa o valorBaseDefault em vez de 100m
                var rep = Clamp(valorBaseDefault - totalPen + bonusConclusao, 0m, 100m);
                await _repo.AtualizarReputacaoAsync(idUsuario, rep);
                return (rep, totalDiasBloqueio);
            }

            decimal somaPesos = 0m;
            decimal soma = 0m;
            decimal peso = 1.0m;

            foreach (var a in avals)
            {
                soma += a.Nota * peso;
                somaPesos += peso;
                peso = Math.Max(0.5m, peso - 0.02m);
            }

            // Se não tiver avaliações, usa o padrão, senão usa a média
            var media = somaPesos > 0 ? soma / somaPesos : valorBaseDefault;

            // O Clamp garante que, mesmo com o bônus, nunca passe de 100
            var reputacao = Clamp(media - totalPen + bonusConclusao, 0m, 100m);

            await _repo.AtualizarReputacaoAsync(idUsuario, reputacao);

            return (reputacao, totalDiasBloqueio);
        }

        private static decimal Clamp(decimal v, decimal min, decimal max)
            => v < min ? min : (v > max ? max : v);
    }
}