using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Repositories.Interfaces;
//using Backend.Utils;
using Backend.Models;
using Backend.DTOs;


namespace Backend.Services
{
    public class AuthService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IAuthGuardRepository _authGuard;

        public AuthService(IUsuarioRepository usuarioRepo, TokenService tokenService, IConfiguration config, IAuthGuardRepository authGuard)
        {
            _usuarioRepo = usuarioRepo;
            _tokenService = tokenService;
            _config = config;
            _authGuard = authGuard;
        }
        public string GerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario) // "Criador" ou "Aventureiro"
            };

            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepo.GetByEmailAsync(request.Email);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha);
            if (!senhaValida)
                throw new UnauthorizedAccessException("Senha inválida.");

            // 🚫 Bloqueio de login ativo?
            if (await _authGuard.HasActiveBlockAsync(usuario.Id))
            {
                var rest = await _authGuard.GetActiveBlockRemainingDaysAsync(usuario.Id);
                var msg = rest.HasValue
                    ? $"Usuário bloqueado para login. Restam {rest.Value} dia(s)."
                    : "Usuário bloqueado para login.";
                throw new UnauthorizedAccessException(msg);
            }

            var token = _tokenService.GenerateToken(usuario);

            return new LoginResponse
            {
                Token = token,
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                TipoUsuario = usuario.TipoUsuario
            };
        }
    }
}
