using System.Security.Claims;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/usuarios/{idUsuario:int}/penalidades")]
    public class PenalidadesController : ControllerBase
    {
        private readonly PenalidadeQueryService _service;
        public PenalidadesController(PenalidadeQueryService service) => _service = service;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Listar(
            int idUsuario,
            [FromQuery] string? tipo,
            [FromQuery] int? idMissaoAceita,
            [FromQuery] DateTime? de,
            [FromQuery] DateTime? ate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var solicitanteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var resp = await _service.ListarAsync(
                    solicitanteId, idUsuario, tipo, idMissaoAceita, de, ate, page, pageSize);
                return Ok(resp);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
