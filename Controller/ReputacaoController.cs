using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReputacaoController : ControllerBase
    {
        private readonly ReputacaoService _service;
        public ReputacaoController(ReputacaoService service) => _service = service;

        // Força o recálculo (útil em testes)
        [Authorize]
        [HttpPost("{idUsuario:int}/recalcular")]
        public async Task<IActionResult> Recalcular(int idUsuario)
        {
            var rep = await _service.RecalcularAsync(idUsuario);
            return Ok(new { usuarioId = idUsuario, reputacao = rep });
        }
    }
}
