using System.Security.Claims;
using Backend.DTOs.Contratos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/contratos")]
    public class ContratosController : ControllerBase
    {
        private readonly ContratoService _service;
        public ContratosController(ContratoService service) => _service = service;

        // Gera (ou atualiza) o contrato para uma MissaoAceita
        [Authorize]
        [HttpPost("gerar/{idMissaoAceita:int}")]
        public async Task<IActionResult> Gerar(int idMissaoAceita, [FromBody] ContratoGenerateRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var resp = await _service.GerarAsync(idMissaoAceita, userId, request);
                return Ok(resp);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // Detalhes do contrato (sem arquivo)
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obter(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var resp = await _service.ObterAsync(id, userId);
                if (resp == null) return NotFound();
                return Ok(resp);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // Download do “PDF”
        [Authorize]
        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.BaixarAsync(id, userId);
                if (result == null) return NotFound(new { message = "Contrato não encontrado ou sem arquivo." });

                return File(result.Value.bytes, "application/pdf", result.Value.fileName);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException) { return NotFound(); }
        }
        [Authorize]
        [HttpGet("{id:int}/preview")]
        public async Task<IActionResult> Preview(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.BaixarAsync(id, userId);
                if (result == null) return NotFound(new { message = "Contrato não encontrado ou sem arquivo." });

                Response.Headers["Content-Disposition"] = $"inline; filename=\"{result.Value.fileName}\"";
                return File(result.Value.bytes, "application/pdf");
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (KeyNotFoundException) { return NotFound(); }
        }
    }
}
