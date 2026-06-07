using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ReputacaoService
    {
        private readonly IReputacaoRepository _repo;
        public ReputacaoService(IReputacaoRepository repo) => _repo = repo;

        public async Task<(decimal Reputacao, int BloqueioDias)> RecalcularAsync(int idUsuario)
        {
            const decimal valorBaseDefault = 50m;

            // Trocamos a chamada para trazer a avaliação + o nível da missão
            var avalsComNivel = await _repo.ListarAvaliacoesComNivelAsync(idUsuario, max: 50);
            var totalPen = await _repo.SomatorioPenalidadesAsync(idUsuario);
            var totalDiasBloqueio = await _repo.SomatorioDiasBloqueioAsync(idUsuario);
            var totalConcluidas = await _repo.ContarMissoesConcluidasAsync(idUsuario);

            // Bônus base bem pequeno apenas pela dedicação
            decimal bonusConclusao = totalConcluidas * 1m;

            decimal saldoAvaliacoes = 0m;
            foreach (var item in avalsComNivel)
            {
                int notaInt = (int)Math.Round(item.avaliacao.Nota);

                // Base das estrelas
                decimal delta = notaInt switch
                {
                    1 => -5m,
                    2 => -2m,
                    3 => 1m,
                    4 => 3m,
                    5 => 5m,
                    _ => 0m
                };

                // A MÁGICA: Bônus de Dificuldade se o trabalho foi bem feito!
                if (notaInt >= 3)
                {
                    delta += item.nivelMinimo * 0.5m; // 0.5 pontos extras por cada nível da missão
                }

                saldoAvaliacoes += delta;
            }

            var reputacao = Clamp(valorBaseDefault + saldoAvaliacoes - totalPen + bonusConclusao, 0m, 100m);
            await _repo.AtualizarReputacaoAsync(idUsuario, reputacao);

            return (reputacao, totalDiasBloqueio);
        }


        private static decimal Clamp(decimal v, decimal min, decimal max)
            => v < min ? min : (v > max ? max : v);
    }
}