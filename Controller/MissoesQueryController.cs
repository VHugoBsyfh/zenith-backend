using Backend.DTOs.Missoes;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/missoes")]
    public class MissoesQueryController : ControllerBase
    {
        private readonly MissaoQueryService _service;
        public MissoesQueryController(MissaoQueryService service) => _service = service;

        // Lista filtrada (pode ser pública; se quiser, adicione [Authorize])
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? tipo,
            [FromQuery] string? localizacao,
            [FromQuery] string? classePreferida,
            [FromQuery] decimal? recompensaMin,
            [FromQuery] decimal? recompensaMax,
            [FromQuery] int? nivelMaxUsuario,
            [FromQuery] string? status = "Disponível",
            [FromQuery] string? sortBy = "data",
            [FromQuery] string? sortDir = "desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var filtro = new MissaoFiltroRequest
            {
                Tipo = tipo,
                Localizacao = localizacao,
                ClassePreferida = classePreferida,
                RecompensaMin = recompensaMin,
                RecompensaMax = recompensaMax,
                NivelMaxUsuario = nivelMaxUsuario,
                Status = status,
                SortBy = sortBy,
                SortDir = sortDir,
                Page = page,
                PageSize = pageSize
            };

            var resp = await _service.ListarAsync(filtro);
            return Ok(resp);
        }
    }
}
