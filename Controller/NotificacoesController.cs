using System.Security.Claims;
using Backend.DTOs.Notificacoes;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/notificacoes")]
    public class NotificacoesController : ControllerBase
    {
        private readonly NotificacaoService _service;
        public NotificacoesController(NotificacaoService service) => _service = service;

        // (uso interno/admin) criar notificação manual
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] NotificacaoCreateRequest req)
        {
            // Opcional: restringir por role/admin
            var resp = await _service.CriarAsync(req);
            return CreatedAtAction(nameof(Listar), new { somenteNaoLidas = false }, resp);
        }

        // listar notificações do logado
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool somenteNaoLidas = true, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (totalNaoLidas, items) = await _service.ListarAsync(userId, somenteNaoLidas, page, pageSize);
            return Ok(new { totalNaoLidas, items });
        }

        // marcar como lida
        [Authorize]
        [HttpPost("{id:int}/lida")]
        public async Task<IActionResult> MarcarLida(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ok = await _service.MarcarComoLidaAsync(id, userId);
            if (!ok) return NotFound(new { message = "Notificação não encontrada." });
            return NoContent();
        }
    }
}
