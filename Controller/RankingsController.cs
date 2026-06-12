using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/rankings")]
    public class RankingsController : ControllerBase
    {
        private readonly RankingService _service;
        public RankingsController(RankingService service) => _service = service;

        [Authorize]
        [HttpGet("aventureiros")]
        public async Task<IActionResult> TopAventureiros(
            [FromQuery] int take = 10,
            [FromQuery] int minMissoes = 0, // Mudei o default para 0, para os novatos aparecerem!
            [FromQuery] string? orderBy = "reputacao") // <-- NOVO PARÂMETRO
        {
            var list = await _service.TopAventureirosAsync(take, minMissoes, orderBy);
            return Ok(list);
        }

        [Authorize]
        [HttpGet("grupos")]
        public async Task<IActionResult> TopGrupos(
            [FromQuery] int take = 10,
            [FromQuery] int minMissoes = 0)
        {
            var list = await _service.TopGruposAsync(take, minMissoes);
            return Ok(list);
        }
    }
}
