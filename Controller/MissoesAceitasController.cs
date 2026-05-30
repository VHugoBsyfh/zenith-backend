using System.Security.Claims;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/missoesaceitas")]
    public class MissoesAceitasController : ControllerBase
    {
        private readonly ConclusaoService _service;
        public MissoesAceitasController(ConclusaoService service) => _service = service;

        [Authorize]
        [HttpPut("{id:int}/concluir")]
        public async Task<IActionResult> Concluir(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.ConcluirAsync(id, userId);

                return Ok(new { message = "Missão concluída com sucesso! Aventureiro(s) ganhou(aram) 10 de reputação." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            // ▼ CORREÇÃO DO ERRO 500 AQUI ▼
            catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }
        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarMissaoRequest request, [FromServices] CancelamentoService service)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await service.CancelarAsync(id, userId, request);
                return Ok(new { message = "Missão cancelada.", penalidade = new { reputacaoPerdida = request.ReputacaoPerdida, bloqueioDias = request.BloqueioDias } });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // ▼ A CORREÇÃO ESTÁ AQUI ▼
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
