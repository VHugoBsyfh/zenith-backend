using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ConclusaoService
    {
        private readonly IConclusaoRepository _repo;
        private readonly IMissaoRepository _missoes;
        private readonly IGrupoRepository _grupos;

        public ConclusaoService(IConclusaoRepository repo, IMissaoRepository missoes, IGrupoRepository grupos)
        {
            _repo = repo;
            _missoes = missoes;
            _grupos = grupos;
        }

        public async Task ConcluirAsync(int idMissao, int solicitanteId)
        {
            var missaoAceita = await _repo.GetAtivaAsync(idMissao)
                ?? throw new KeyNotFoundException("Missão não encontrada ou já concluída.");

            bool autorizado = false;

            if (missaoAceita.IdUsuario.HasValue)
                autorizado = missaoAceita.IdUsuario == solicitanteId;
            else if (missaoAceita.IdGrupo.HasValue)
                autorizado = await _grupos.IsMembroAsync(missaoAceita.IdGrupo.Value, solicitanteId);

            if (!autorizado)
                throw new UnauthorizedAccessException("Você não está autorizado a concluir esta missão.");

            await _repo.AtualizarStatusAsync(missaoAceita, "Concluída");
            await _missoes.SetStatusAsync(missaoAceita.IdMissao, "Concluída");

            // var historico = new HistoricoMissao
            // {
            //     IdUsuario = solicitanteId,
            //     //IdMissaoAceita = idMissaoAceita,
            //     Resultado = "Concluída",
            //     DataRegistro = DateTime.Now
            // };

            //await _repo.RegistrarHistoricoAsync(historico);

            // ▼ NOVA PARTE: GANHO DE REPUTAÇÃO (+10) ▼
            decimal ganhoReputacao = 10.0m;

            if (missaoAceita.IdUsuario.HasValue)
            {
                await _repo.AjustarReputacaoAsync(missaoAceita.IdUsuario.Value, ganhoReputacao);
            }
            else if (missaoAceita.IdGrupo.HasValue)
            {
                // Se for em grupo, dá +10 para cada membro
                var membros = await _repo.ListarMembrosDoGrupoAsync(missaoAceita.IdGrupo.Value);
                foreach (var idMembro in membros)
                {
                    await _repo.AjustarReputacaoAsync(idMembro, ganhoReputacao);
                }
            }
        }
    }
}
