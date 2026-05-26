using System.Security.Claims;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvaliacoesController : ControllerBase
    {
        private readonly AvaliacaoService _service;
        public AvaliacoesController(AvaliacaoService service) => _service = service;

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] AvaliacaoCreateRequest request)
        {
            try
            {
                var avaliadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var resp = await _service.CriarAsync(avaliadorId, request);
                return CreatedAtAction(nameof(ListarPorMissao), new { idMissaoAceita = resp.IdMissaoAceita }, resp);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)   { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex)        { return NotFound(new { message = ex.Message }); }
        }

        [Authorize]
        [HttpGet("missao/{idMissaoAceita:int}")]
        public async Task<IActionResult> ListarPorMissao(int idMissaoAceita)
            => Ok(await _service.ListarPorMissaoAsync(idMissaoAceita));

        [Authorize]
        [HttpGet("usuario/{idUsuario:int}/recebidas")]
        public async Task<IActionResult> ListarRecebidas(int idUsuario)
            => Ok(await _service.ListarRecebidasDoUsuarioAsync(idUsuario));
    }
}
