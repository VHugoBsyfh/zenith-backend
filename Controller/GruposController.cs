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
    public class GruposController : ControllerBase
    {
        private readonly GrupoService _service;

        public GruposController(GrupoService service)
        {
            _service = service;
        }

        [Authorize] // qualquer usuário autenticado
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] GrupoCreateRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.CriarAsync(request, userId);
            return CreatedAtAction(nameof(ListarMembros), new { id = result.Id }, result);
        }

        [Authorize]
        [HttpPost("{id}/membros")]
        public async Task<IActionResult> AdicionarMembro(int id, [FromBody] AddMembroRequest req)
        {
            var solicitanteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // por Id direto (simples)
            if (req.IdUsuario <= 0)
                return BadRequest(new { message = "Informe IdUsuario válido." });

            await _service.AdicionarMembroAsync(id, solicitanteId, req.IdUsuario);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}/membros/me")]
        public async Task<IActionResult> SairDoGrupo(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.RemoverProprioAcessoAsync(id, userId);
            return NoContent();
        }

        [Authorize]
        [HttpGet("{id}/membros")]
        public async Task<IActionResult> ListarMembros(int id)
        {
            var membros = await _service.ListarMembrosAsync(id);
            return Ok(membros.Select(m => new { m.Id, m.Nome, m.Email, m.Classe, m.Nivel, m.Reputacao }));
        }
        [Authorize]
        [HttpPost("{idGrupo:int}/missoes/{idMissao:int}/aceitar")]
        public async Task<IActionResult> AceitarGrupo(int idGrupo, int idMissao, [FromServices] AceitacaoService aceitacao, [FromServices] IMissaoRepository missoesRepo)
        {
            try
            {
                var solicitanteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var missao = await missoesRepo.GetByIdForUpdateAsync(idMissao) ?? throw new KeyNotFoundException("Missão não encontrada.");
                var registroId = await aceitacao.AceitarGrupoAsync(idMissao, idGrupo, solicitanteId, missao.IdCriador);
                return Created($"/api/missoesaceitas/{registroId}", new { id = registroId, idMissao = idMissao, idGrupo, tipo = "grupo" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }
        //
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ListarGrupos([FromQuery] int? id)
        {
            var grupos = await _service.ListarGruposAsync(id);

            if (id.HasValue && !grupos.Any())
                return NotFound(new { message = "Grupo não encontrado." });

            return Ok(grupos);
        }
        //
        [Authorize]
        [HttpPut("{idGrupo:int}/missoes/{idMissao:int}/concluir")]
        public async Task<IActionResult> ConcluirGrupo(
    int idGrupo,
    int idMissao,
    [FromServices] AceitacaoService aceitacao)
        {
            try
            {
                var solicitanteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                await aceitacao.ConcluirGrupoAsync(idMissao, idGrupo, solicitanteId);

                return Ok(new { message = "Missão concluída com sucesso por todo o grupo!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
