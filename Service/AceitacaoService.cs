using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class AceitacaoService
    {
        private readonly IMissaoRepository _missoes;
        private readonly IMissaoAceitaRepository _aceites;
        private readonly IGrupoRepository _grupos;

        public AceitacaoService(IMissaoRepository missoes, IMissaoAceitaRepository aceites, IGrupoRepository grupos)
        {
            _missoes = missoes;
            _aceites = aceites;
            _grupos = grupos;
        }

        // Solo
        public async Task<int> AceitarSoloAsync(
    int idMissao,
    int idUsuario,
    string status,
    int? idCriadorDaMissao = null)
        {
            var missao = await _missoes.GetByIdForUpdateAsync(idMissao)
                         ?? throw new KeyNotFoundException("Missão não encontrada.");

            // if (await _aceites.ExisteEmAndamentoParaUsuarioAsync(idUsuario))
            //     throw new InvalidOperationException("Você já possui uma missão em andamento.");

            // if (!string.Equals(missao.Status, "Disponível", StringComparison.OrdinalIgnoreCase))
            //     throw new InvalidOperationException("Missão não está disponível para aceite.");

            // if (idCriadorDaMissao.HasValue && idCriadorDaMissao.Value == idUsuario)
            //     throw new InvalidOperationException("Criador não pode aceitar a própria missão.");

            // if (await _aceites.UsuarioTemAtivaAsync(idUsuario))
            //     throw new InvalidOperationException("Usuário já possui missão em andamento.");

            // if (await _aceites.MissaoJaFoiAceitaAsync(idMissao))
            //     throw new InvalidOperationException("Missão já foi aceita por outro participante.");

            var registro = await _aceites.CriarAsync(new MissaoAceita
            {
                IdMissao = idMissao,
                IdUsuario = idUsuario,
                IdGrupo = null,
                StatusMissao = status
            });

            await _missoes.VincularAventureiroAsync(idMissao, idUsuario, status);
            return registro.Id;
        }

        // Grupo
        // Grupo
        public async Task<int> AceitarGrupoAsync(int idMissao, int idGrupo, int solicitanteId, int? idCriadorDaMissao = null)
        {
            var missao = await _missoes.GetByIdForUpdateAsync(idMissao)
                         ?? throw new KeyNotFoundException("Missão não encontrada.");

            if (!string.Equals(missao.Status, "Disponível", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Missão não está disponível para aceite.");

            // ▼ NOVA VALIDAÇÃO: Impede que o criador aceite a própria missão através de um grupo
            var membrosDoGrupo = await _grupos.ListarMembrosAsync(idGrupo);
            if (membrosDoGrupo.Any(m => m.Id == missao.IdCriador))
                throw new InvalidOperationException("O criador da missão não pode fazer parte do grupo que a aceita.");

            if (await _aceites.ExisteEmAndamentoParaAlgumMembroDoGrupoAsync(idGrupo))
                throw new InvalidOperationException("Algum membro do grupo já possui missão em andamento.");

            if (!await _grupos.IsMembroAsync(idGrupo, solicitanteId))
                throw new UnauthorizedAccessException("Apenas membros do grupo podem aceitar missões pelo grupo.");

            var membrosCount = await _grupos.CountMembrosAsync(idGrupo);
            if (membrosCount < 2)
                throw new InvalidOperationException("Grupo precisa ter pelo menos 2 membros para aceitar missão.");

            // RN002 para o grupo: checar se o grupo já tem missão ativa
            if (await _aceites.GrupoTemAtivaAsync(idGrupo))
                throw new InvalidOperationException("O grupo já possui missão em andamento.");

            if (await _aceites.MissaoJaFoiAceitaAsync(idMissao))
                throw new InvalidOperationException("Missão já foi aceita por outro participante.");

            var registro = await _aceites.CriarAsync(new MissaoAceita
            {
                IdMissao = idMissao,
                IdGrupo = idGrupo,
                IdUsuario = null,
                StatusMissao = "Em andamento"
            });

            await _missoes.SetStatusAsync(idMissao, "Aceita");
            return registro.Id;
        }
    }
}
