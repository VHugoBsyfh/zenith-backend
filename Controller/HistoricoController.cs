using System.Security.Claims;
using Backend.DTOs.Historico;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/usuarios/{idUsuario:int}/historico")]
    public class HistoricoController : ControllerBase
    {
        private readonly HistoricoService _service;
        public HistoricoController(HistoricoService service) => _service = service;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Listar(
            int idUsuario,
            [FromQuery] string? resultado,
            [FromQuery] DateTime? de,
            [FromQuery] DateTime? ate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var solicitanteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var role = User.FindFirstValue(ClaimTypes.Role);

                var filtro = new HistoricoFiltroRequest
                {
                    Resultado = resultado,
                    De = de,
                    Ate = ate,
                    Page = page,
                    PageSize = pageSize
                };

                var resp = await _service.ListarAsync(solicitanteId, idUsuario, filtro, role);
                return Ok(resp);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }
    }
}
