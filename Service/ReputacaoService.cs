using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ReputacaoService
    {
        private readonly IReputacaoRepository _repo;
        public ReputacaoService(IReputacaoRepository repo) => _repo = repo;

        // Alteramos o retorno para uma Tupla
        public async Task<(decimal Reputacao, int BloqueioDias)> RecalcularAsync(int idUsuario)
        {
            var avals = await _repo.ListarAvaliacoesRecebidasAsync(idUsuario, max: 50);

            // Busca as penalidades e os dias de bloqueio em paralelo (opcional, mas eficiente)
            var totalPen = await _repo.SomatorioPenalidadesAsync(idUsuario);
            var totalDiasBloqueio = await _repo.SomatorioDiasBloqueioAsync(idUsuario);

            if (avals.Count == 0)
            {
                // Se não tem avaliações, base é 100 menos as penalidades
                var rep = Clamp(100m - totalPen, 0m, 100m); // <-- Clamp ajustado para 100m
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

            var media = somaPesos > 0 ? soma / somaPesos : 100m;
            var reputacao = Clamp(media - totalPen, 0m, 100m);

            await _repo.AtualizarReputacaoAsync(idUsuario, reputacao);

            // Retorna os dois valores
            return (reputacao, totalDiasBloqueio);
        }

        private static decimal Clamp(decimal v, decimal min, decimal max)
            => v < min ? min : (v > max ? max : v);
    }
}