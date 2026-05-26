using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class CancelamentoService
    {
        private readonly ICancelamentoRepository _repo;
        private readonly IGrupoRepository _grupos;
        private readonly ReputacaoService _reputacaoService;

        public CancelamentoService(ICancelamentoRepository repo, IGrupoRepository grupos, ReputacaoService reputacaoService)
        {
            _repo = repo;
            _grupos = grupos;
            _reputacaoService = reputacaoService;
        }

        public async Task CancelarAsync(int idMissaoAceita, int solicitanteId, CancelarMissaoRequest req)
        {
            var reg = await _repo.GetAtivaAsync(idMissaoAceita)
                      ?? throw new KeyNotFoundException("Missão não encontrada ou não está em andamento.");

            // Autorização: quem aceitou pode cancelar
            bool autorizado = false;
            if (reg.IdUsuario.HasValue)
                autorizado = reg.IdUsuario.Value == solicitanteId;
            else if (reg.IdGrupo.HasValue)
                autorizado = await _grupos.IsMembroAsync(reg.IdGrupo.Value, solicitanteId);

            if (!autorizado)
                throw new UnauthorizedAccessException("Você não está autorizado a cancelar esta missão.");

            // Atualiza status para Cancelada (aceitação e missão)
            await _repo.AtualizarParaCanceladaAsync(reg, req.Motivo);

            // Penalidades
            var delta = -Math.Abs(req.ReputacaoPerdida); // sempre negativo
            if (reg.IdUsuario.HasValue)
            {
                await AplicarPenalidadeUsuario(reg.IdUsuario.Value, reg.Id, req, delta);
            }
            else if (reg.IdGrupo.HasValue)
            {
                var membros = await _repo.ListarMembrosDoGrupoAsync(reg.IdGrupo.Value);
                foreach (var idMembro in membros)
                    await AplicarPenalidadeUsuario(idMembro, reg.Id, req, delta);
            }
            
        }

        private async Task AplicarPenalidadeUsuario(int idUsuario, int idMissaoAceita, CancelarMissaoRequest req, decimal deltaRep)
        {
            await _repo.AjustarReputacaoAsync(idUsuario, deltaRep);

            var p = new Penalidade
            {
                IdUsuario = idUsuario,
                IdMissaoAceita = idMissaoAceita,
                TipoPenalidade = "ReducaoReputacao",
                ValorPenalidade = Math.Abs(deltaRep),
                DuracaoBloqueioDias = req.BloqueioDias,
                Justificativa = req.Motivo
            };

            await _repo.RegistrarPenalidadeAsync(p);
            await _reputacaoService.RecalcularAsync(idUsuario);
        }
    }
}
