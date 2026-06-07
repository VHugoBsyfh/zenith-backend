using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
namespace Backend.Services
{
    public class MissaoService
    {
        private readonly IMissaoRepository _repo;
        private readonly IUsuarioRepository _usuarios;

        public MissaoService(IMissaoRepository repo, IUsuarioRepository usuarios)
        {
            _repo = repo;
            _usuarios = usuarios;
        }

        public async Task<MissaoResponse> CriarAsync(MissaoCreateRequest req, int idCriador)
        {
            var missao = new Missao
            {
                Titulo = req.Titulo,
                Descricao = req.Descricao,
                Localizacao = req.Localizacao,
                Recompensa = req.Recompensa,
                TipoMissao = req.TipoMissao,
                NivelMinimo = req.NivelMinimo,
                ClassePreferida = req.ClassePreferida,
                IdCriador = idCriador
            };

            var created = await _repo.AddAsync(missao);

            return new MissaoResponse
            {
                Id = created.Id,
                Titulo = created.Titulo,
                TipoMissao = created.TipoMissao,
                Localizacao = created.Localizacao,
                Recompensa = created.Recompensa,
                Status = created.Status,
                NivelMinimo = created.NivelMinimo,
                ClassePreferida = created.ClassePreferida,
                DataCriacao = created.DataCriacao
            };
        }
        public async Task<IEnumerable<Missao>> FiltrarAsync(
    string? tipo,
    string? localizacao,
    string? classe,
    int? nivelMaximo,
    decimal? recompensaMinima,
    int? idCriador,
    int? idAventureiro)
        {
            return await _repo.FiltrarAsync(
                tipo,
                localizacao,
                classe,
                nivelMaximo,
                recompensaMinima,
                idCriador,
                idAventureiro);
        }
        // public async Task<IEnumerable<Missao>> RecomendadasAsync(int usuarioId, int top = 10)
        // {
        //     var u = await _usuarios.GetByIdAsync(usuarioId)
        //             ?? throw new KeyNotFoundException("Usuário não encontrado.");
        //     return await _repo.RecomendadasAsync(u.Classe, u.Nivel, top);
        // }
        public async Task<MissaoResponse> AtualizarAsync(int id, MissaoUpdateRequest req, int userId)
        {
            // 1) carregar missão
            var missao = await _repo.GetByIdForUpdateAsync(id)
                         ?? throw new KeyNotFoundException("Missão não encontrada.");

            // 2) autoria
            if (missao.IdCriador != userId)
                throw new UnauthorizedAccessException("Apenas o criador pode editar esta missão.");

            // 3) status permitido
            if (!string.Equals(missao.Status, "Disponível", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Missão só pode ser editada quando estiver 'Disponível'.");

            // 4) validação básica (já tem DataAnnotations no DTO; aqui garantimos em runtime)
            if (string.IsNullOrWhiteSpace(req.Titulo) ||
                string.IsNullOrWhiteSpace(req.Descricao) ||
                string.IsNullOrWhiteSpace(req.Localizacao) ||
                string.IsNullOrWhiteSpace(req.TipoMissao) ||
                req.NivelMinimo < 1 || req.Recompensa < 0)
                throw new ValidationException("Dados inválidos para atualização.");

            // 5) aplicar mudanças (campos permitidos)
            missao.Titulo = req.Titulo;
            missao.Descricao = req.Descricao;
            missao.Localizacao = req.Localizacao;
            missao.Recompensa = req.Recompensa;
            missao.TipoMissao = req.TipoMissao;
            missao.NivelMinimo = req.NivelMinimo;
            missao.ClassePreferida = req.ClassePreferida;

            await _repo.UpdateAsync(missao);

            // 6) mapear resposta
            return new MissaoResponse
            {
                Id = missao.Id,
                Titulo = missao.Titulo,
                TipoMissao = missao.TipoMissao,
                Localizacao = missao.Localizacao,
                Recompensa = missao.Recompensa,
                Status = missao.Status,
                NivelMinimo = missao.NivelMinimo,
                ClassePreferida = missao.ClassePreferida,
                DataCriacao = missao.DataCriacao
            };
        }
        public async Task ExcluirAsync(int id, int userId)
        {
            var missao = await _repo.GetByIdForUpdateAsync(id)
                         ?? throw new KeyNotFoundException("Missão não encontrada.");

            if (missao.IdCriador != userId)
                throw new UnauthorizedAccessException("Apenas o criador pode excluir esta missão.");

            if (!string.Equals(missao.Status, "Disponível", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Só é possível excluir missões em status 'Disponível'.");

            if (await _repo.HasVinculosAsync(id))
                throw new InvalidOperationException("Missão já possui vínculos (aceitação/andamento) e não pode ser excluída.");

            await _repo.DeleteAsync(id);
        }



        public async Task<IEnumerable<Missao>> ListarAsync() => await _repo.GetAllAsync();
        public async Task<Missao?> BuscarPorIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task AtualizarAsync(Missao missao) => await _repo.UpdateAsync(missao);
        public async Task ExcluirAsync(int id) => await _repo.DeleteAsync(id);
    }
}
