using System.ComponentModel.DataAnnotations;
using Backend.DTOs.Admin;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class UsuarioAdminService
    {
        private readonly IUsuarioAdminRepository _repo;
        public UsuarioAdminService(IUsuarioAdminRepository repo) => _repo = repo;

        public async Task<(int total, List<UsuarioAdminListResponse> items)> ListarAsync(
            string? q, string? tipo, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
            return await _repo.ListarAsync(q, tipo, page, pageSize);
        }

        public Task AtualizarTipoAsync(int id, string novoTipo)
        {
            var permitido = new[] { "Aventureiro", "Criador", "Admin" };
            if (!permitido.Contains(novoTipo))
                throw new ValidationException("Tipo de usuário inválido.");
            return _repo.AtualizarTipoAsync(id, novoTipo);
        }

        public async Task ResetarSenhaAsync(int id, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
                throw new ValidationException("Senha deve ter pelo menos 6 caracteres.");
            var hash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            await _repo.ResetarSenhaAsync(id, hash);
        }
    }
}
