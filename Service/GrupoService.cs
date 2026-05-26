using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class GrupoService
    {
        private readonly IGrupoRepository _grupos;
        private readonly IUsuarioRepository _usuarios;

        public GrupoService(IGrupoRepository grupos, IUsuarioRepository usuarios)
        {
            _grupos = grupos;
            _usuarios = usuarios;
        }

        public async Task<GrupoResponse> CriarAsync(GrupoCreateRequest req, int userId)
        {
            var grupo = await _grupos.CriarAsync(new Grupo { NomeGrupo = req.NomeGrupo });

            // criador entra automaticamente como membro
            await _grupos.AddMembroAsync(grupo.Id, userId);

            return new GrupoResponse
            {
                Id = grupo.Id,
                NomeGrupo = grupo.NomeGrupo,
                QuantidadeMembros = await _grupos.CountMembrosAsync(grupo.Id),
                IdMissaoVinculada = grupo.IdMissaoVinculada,
                DataCriacao = grupo.DataCriacao
            };
        }

        public async Task AdicionarMembroAsync(int idGrupo, int solicitanteId, int novoMembroId)
        {
            // só quem é membro pode convidar/adicionar
            if (!await _grupos.IsMembroAsync(idGrupo, solicitanteId))
                throw new UnauthorizedAccessException("Apenas membros do grupo podem adicionar usuários.");

            // não adicionar duplicado
            if (await _grupos.IsMembroAsync(idGrupo, novoMembroId))
                throw new InvalidOperationException("Usuário já é membro deste grupo.");

            // confirmar que usuário existe
            var user = await _usuarios.GetByIdAsync(novoMembroId)
                       ?? throw new KeyNotFoundException("Usuário não encontrado.");

            await _grupos.AddMembroAsync(idGrupo, novoMembroId);
        }

        public async Task RemoverProprioAcessoAsync(int idGrupo, int userId)
        {
            if (!await _grupos.IsMembroAsync(idGrupo, userId))
                throw new KeyNotFoundException("Você não é membro deste grupo.");

            await _grupos.RemoveMembroAsync(idGrupo, userId);
        }

        public async Task<IEnumerable<Usuario>> ListarMembrosAsync(int idGrupo)
            => await _grupos.ListarMembrosAsync(idGrupo);
    }
}
