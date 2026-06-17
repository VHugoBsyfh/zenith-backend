using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Backend.Repositories.Interfaces;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IUsuarioService service, ILogger<UsuariosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Cadastra um novo usuário (Aventureiro/Criador).</summary>
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegisterUserRequest request)
        {
            try
            {
                var result = await _service.RegistrarAsync(request);
                _logger.LogInformation("Usuário criado: {Email}", result.Email);
                return CreatedAtAction(nameof(Registrar), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // e-mail duplicado
                return Conflict(new { message = ex.Message });
            }
        }
        //
        [Authorize(Roles = "Aventureiro")]
        [HttpPatch("me/nivel")]
        public async Task<IActionResult> AtualizarNivel([FromBody] AtualizarNivelRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.AtualizarNivelAsync(userId, request.NovoNivel);

                return Ok(new { message = $"Nível atualizado para {request.NovoNivel} com sucesso!" });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
        //
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? role, [FromQuery] int? id)
        {
            try
            {
                var usuarios = await _service.ListarUsuariosAsync(role, id);

                // Se buscou por ID específico e não achou ninguém, retorna 404 bacana
                if (id.HasValue && !usuarios.Any())
                {
                    return NotFound(new { message = "Usuário não encontrado." });
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar usuários.", error = ex.Message });
            }
        }
        //
        [Authorize]
        [HttpGet("{id}/saldo")]
        public async Task<IActionResult> ObterSaldoAcumulado(int id, [FromServices] IUsuarioRepository repo)
        {
            try
            {
                // Opcional: Garantir que o usuário só pode ver o próprio saldo
                var usuarioLogadoId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (usuarioLogadoId != id)
                    return Forbid("Você só pode ver o seu próprio saldo.");

                var saldo = await repo.ObterValorAcumuladoAsync(id);

                return Ok(new { valorAcumulado = saldo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro ao calcular o saldo: " + ex.Message });
            }
        }
    }

}
