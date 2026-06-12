using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class AceitacaoService
    {
        private readonly IMissaoRepository _missoes;
        private readonly IMissaoAceitaRepository _aceites;
        private readonly IGrupoRepository _grupos;
        private readonly ReputacaoService _reputacao;

        public AceitacaoService(IMissaoRepository missoes, IMissaoAceitaRepository aceites, IGrupoRepository grupos,
        ReputacaoService reputacao)
        {
            _missoes = missoes;
            _aceites = aceites;
            _grupos = grupos;
            _reputacao = reputacao;
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
            await _reputacao.RecalcularAsync(idUsuario);
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

            // ▼ A VARIÁVEL É CRIADA E CARREGADA AQUI
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

            // Atualiza o banco principal para o WM conseguir filtrar!
            await _missoes.VincularGrupoAsync(idMissao, idGrupo, "Em andamento");

            // ▼ CORREÇÃO: Apenas fazemos o loop na variável que já existe, sem redeclarar!
            foreach (var membro in membrosDoGrupo)
            {
                await _reputacao.RecalcularAsync(membro.Id);
            }

            return registro.Id;
        }
        //
        public async Task ConcluirGrupoAsync(int idMissao, int idGrupo, int solicitanteId)
        {
            // ... validações iniciais (IsMembroAsync, etc) ...

            var registro = await _aceites.ObterRegistroGrupoAsync(idMissao, idGrupo)
                ?? throw new KeyNotFoundException("O grupo não possui um registro ativo para esta missão.");

            if (registro.StatusMissao == "Concluída")
                throw new InvalidOperationException("A missão já foi concluída anteriormente.");

            // Atualiza a tabela de histórico
            await _aceites.AtualizarStatusRegistroAsync(registro.Id, "Concluída");

            // ▼ AQUI ESTÁ O SEGREDO: Usamos o método que só muda o status e não apaga o IdGrupo!
            await _missoes.SetStatusAsync(idMissao, "Concluída");

            // Recalcula a reputação de todo mundo
            var membros = await _grupos.ListarMembrosAsync(idGrupo);
            foreach (var membro in membros)
            {
                await _reputacao.RecalcularAsync(membro.Id);
            }
        }
    }
}
