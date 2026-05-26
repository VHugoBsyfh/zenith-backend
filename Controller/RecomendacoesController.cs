using System.Security.Claims;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/recomendacoes")]
    public class RecomendacoesController : ControllerBase
    {
        private readonly RecomendacaoService _service;
        public RecomendacoesController(RecomendacaoService service) => _service = service;

        // Recomendadas para o usuário logado
        [Authorize(Roles = "Aventureiro")]
        [HttpGet("missoes")]
        public async Task<IActionResult> Missoes([FromQuery] int take = 10, [FromQuery] string? localizacao = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var list = await _service.RecomendarAsync(userId, take, localizacao);
            return Ok(list);
        }
    }
}
