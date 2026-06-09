using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using BCrypt.Net;

namespace Backend.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserResponse> RegistrarAsync(RegisterUserRequest req)
        {
            // 1) checar e-mail único
            if (await _repo.EmailExisteAsync(req.Email))
                throw new InvalidOperationException("Email já cadastrado.");

            // 2) hash de senha
            var hash = BCrypt.Net.BCrypt.HashPassword(req.Senha);

            // 3) montar entidade
            var usuario = new Usuario
            {
                Nome = req.Nome,
                Email = req.Email,
                Senha = hash,
                Classe = req.Classe,
                TipoUsuario = req.TipoUsuario,
                Nivel = 1,
                Reputacao = 50.0M,
                DataCadastro = DateTime.Now
            };

            // 4) persistir
            var created = await _repo.AddAsync(usuario);

            // 5) mapear resposta (sem senha)
            return new UserResponse
            {
                Id = created.Id,
                Nome = created.Nome,
                Email = created.Email,
                Classe = created.Classe,
                Nivel = created.Nivel,
                TipoUsuario = created.TipoUsuario,
                Reputacao = created.Reputacao,
                DataCadastro = created.DataCadastro
            };
        }
        public async Task AtualizarNivelAsync(int idUsuario, int novoNivel)
        {
            if (novoNivel < 1)
                throw new InvalidOperationException("O nível não pode ser menor que 1.");

            var user = await _repo.GetByIdAsync(idUsuario)
                       ?? throw new KeyNotFoundException("Usuário não encontrado.");

            await _repo.AtualizarNivelAsync(idUsuario, novoNivel);
        }
        public async Task<List<UsuarioResponse>> ListarUsuariosAsync(string? role, int? id)
        {
            var usuarios = await _repo.ListarAsync(role, id);

            return usuarios.Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Classe = u.Classe,
                Nivel = u.Nivel,
                TipoUsuario = u.TipoUsuario,
                Reputacao = u.Reputacao,
                Role = u.TipoUsuario,
                IdGrupo = u.IdGrupo // <--- Mapeando o novo campo
            }).ToList();
        }
    }
}
