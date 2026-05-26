using System.Security.Claims;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/grupos/{idGrupo:int}/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _service;
        public ChatController(ChatService service) => _service = service;

        [Authorize]
        [HttpPost("mensagens")]
        public async Task<IActionResult> Enviar(int idGrupo, [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.EnviarAsync(idGrupo, userId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex)          { return BadRequest(new { message = ex.Message }); }
        }

        [Authorize]
        [HttpGet("mensagens")]
        public async Task<IActionResult> Listar(int idGrupo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.ListarAsync(idGrupo, userId, page, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }
}
