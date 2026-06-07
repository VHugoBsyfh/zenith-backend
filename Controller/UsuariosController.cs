using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

    }
}
