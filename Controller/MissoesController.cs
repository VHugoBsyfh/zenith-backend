using System.Security.Claims;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Backend.Repositories;
using Backend.Repositories.Interfaces;
using Backend.DTOs.Missoes;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MissoesController : ControllerBase
    {
        private readonly MissaoService _service;
        public MissoesController(MissaoService service)
        {
            _service = service;
        }
        //catch (InvalidOperationException ex)
        // {
        //     return Conflict(new { message = ex.Message });
        // }

        [Authorize(Roles = "Criador")]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] MissaoCreateRequest request)
        {
            // 🔹 pega o id do criador a partir do token
            var idCriador = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // 🔹 passa esse id manualmente pro service (o cliente não manda)
            var result = await _service.CriarAsync(request, idCriador);

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Id }, result);
        }

        // [HttpGet]
        // public async Task<IActionResult> Listar()
        // {
        //     var missoes = await _service.ListarAsync();
        //     return Ok(missoes);
        // }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var missao = await _service.BuscarPorIdAsync(id);
            if (missao == null) return NotFound();
            return Ok(missao);
        }

        [HttpPut("atualizar-{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Missao missao)
        {
            if (id != missao.Id) return BadRequest();
            await _service.AtualizarAsync(missao);
            return NoContent();
        }

        // [HttpDelete("{id}")]
        // public async Task<IActionResult> Excluir(int id)
        // {
        //     await _service.ExcluirAsync(id);
        //     return NoContent();
        // }

        [HttpGet("filtrar")]
        public async Task<IActionResult> Filtrar(
    [FromQuery] string? tipo,
    [FromQuery] string? localizacao,
    [FromQuery] string? classe,
    [FromQuery] int? nivelMaximo,
    [FromQuery] decimal? recompensaMinima,
    [FromQuery] int? idCriador,
    [FromQuery] int? idAventureiro)
        {
            var missoes = await _service.FiltrarAsync(
                tipo,
                localizacao,
                classe,
                nivelMaximo,
                recompensaMinima,
                idCriador,
                idAventureiro);

            return Ok(missoes);
        }
        // [Authorize] // opcionalmente: [Authorize(Roles="Aventureiro")]
        // [HttpGet("recomendadas")]
        // public async Task<IActionResult> Recomendadas([FromQuery] int top = 10)
        // {
        //     var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //     var lista = await _service.RecomendadasAsync(userId, top);
        //     return Ok(lista);
        // }
        [Authorize(Roles = "Criador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] MissaoUpdateRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.AtualizarAsync(id, request, userId);
                return Ok(result); // ou 204 se preferir sem corpo
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
            catch (ValidationException ex)
            {
                return UnprocessableEntity(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Criador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.ExcluirAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [Authorize(Roles = "Aventureiro")]
        [HttpPost("{id:int}/aceitar")]
        public async Task<IActionResult> AceitarSolo(
    int id,
    [FromBody] AceitarMissaoRequest request,
    [FromServices] AceitacaoService aceitacao,
    [FromServices] IMissaoRepository missoesRepo)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var missao = await missoesRepo.GetByIdForUpdateAsync(id)
                             ?? throw new KeyNotFoundException("Missão não encontrada.");

                var registroId = await aceitacao.AceitarSoloAsync(
                    id,
                    userId,
                    request.Status,
                    missao.IdCriador
                );

                return Created(
                    $"/api/missoesaceitas/{registroId}",
                    new
                    {
                        id = registroId,
                        idMissao = id,
                        tipo = "solo",
                        status = request.Status
                    });
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
