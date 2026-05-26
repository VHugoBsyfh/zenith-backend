using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ReputacaoService
    {
        private readonly IReputacaoRepository _repo;
        public ReputacaoService(IReputacaoRepository repo) => _repo = repo;

        // Recalcula reputação do usuário a partir de avaliações + penalidades
        public async Task<decimal> RecalcularAsync(int idUsuario)
        {
            var avals = await _repo.ListarAvaliacoesRecebidasAsync(idUsuario, max: 50);

            // média ponderada (peso decai com a posição: 1.0, 0.98, 0.96, ... mínimo 0.5)
            if (avals.Count == 0)
            {
                // Sem avaliações: reputação base 0 (menos penalidades -> clamp)
                var totalPen = await _repo.SomatorioPenalidadesAsync(idUsuario);
                var rep = Clamp(0m - totalPen, 0m, 5m);
                await _repo.AtualizarReputacaoAsync(idUsuario, rep);
                return rep;
            }

            decimal somaPesos = 0m;
            decimal soma = 0m;
            decimal peso = 1.0m;
            foreach (var a in avals)
            {
                soma += a.Nota * peso;
                somaPesos += peso;
                peso = Math.Max(0.5m, peso - 0.02m); // decai até 0.5
            }
            var media = somaPesos > 0 ? soma / somaPesos : 0m;

            var penalidades = await _repo.SomatorioPenalidadesAsync(idUsuario);
            var reputacao = Clamp(media - penalidades, 0m, 5m);

            await _repo.AtualizarReputacaoAsync(idUsuario, reputacao);
            return reputacao;
        }

        private static decimal Clamp(decimal v, decimal min, decimal max)
            => v < min ? min : (v > max ? max : v);
    }
}
