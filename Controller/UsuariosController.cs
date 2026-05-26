using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    }
}
