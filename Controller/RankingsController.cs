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

        // Top Aventureiros
        [Authorize]
        [HttpGet("aventureiros")]
        public async Task<IActionResult> TopAventureiros(
            [FromQuery] int take = 10,
            [FromQuery] int minMissoes = 1)
        {
            var list = await _service.TopAventureirosAsync(take, minMissoes);
            return Ok(list);
        }

        // Top Grupos
        [Authorize]
        [HttpGet("grupos")]
        public async Task<IActionResult> TopGrupos(
            [FromQuery] int take = 10,
            [FromQuery] int minMissoes = 1)
        {
            var list = await _service.TopGruposAsync(take, minMissoes);
            return Ok(list);
        }
    }
}
