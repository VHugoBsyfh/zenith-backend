using System.Security.Claims;
using Backend.DTOs;
using Backend.Repositories.Interfaces;
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
        private readonly IGrupoRepository gruposRepo;
        public AvaliacoesController(AvaliacaoService service, IGrupoRepository gruposRepo) => _service = service;

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
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [Authorize]
        [HttpGet("missao/{idMissaoAceita:int}")]
        public async Task<IActionResult> ListarPorMissao(int idMissaoAceita)
            => Ok(await _service.ListarPorMissaoAsync(idMissaoAceita));

        [Authorize]
        [HttpGet("usuario/{idUsuario:int}/recebidas")]
        public async Task<IActionResult> ListarRecebidas(int idUsuario)
            => Ok(await _service.ListarRecebidasDoUsuarioAsync(idUsuario));

        [Authorize]
        [HttpPost("grupo")]
        public async Task<IActionResult> AvaliarGrupo(
    [FromBody] AvaliacaoGrupoCreateRequest request,
    [FromServices] IGrupoRepository gruposRepo)
        {
            try
            {
                var avaliadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                await _service.AvaliarGrupoAsync(avaliadorId, request, gruposRepo);

                return Ok(new { message = "Grupo avaliado com sucesso! A reputação da guilda foi atualizada." });
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
