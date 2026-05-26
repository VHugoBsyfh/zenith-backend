using Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/usuarios/{idUsuario:int}/bloqueio")]
    [Authorize(Roles = "Admin")]
    public class AdminBloqueiosController : ControllerBase
    {
        private readonly IAuthGuardRepository _repo;
        public AdminBloqueiosController(IAuthGuardRepository repo) => _repo = repo;

        public class AplicarBloqueioRequest
        {
            public int Dias { get; set; } = 7;
            public string? Motivo { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Aplicar(int idUsuario, [FromBody] AplicarBloqueioRequest req)
        {
            if (req.Dias <= 0) return UnprocessableEntity(new { message = "Dias deve ser >= 1." });
            await _repo.ApplyBlockAsync(idUsuario, req.Dias, req.Motivo);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Remover(int idUsuario)
        {
            var ok = await _repo.RemoveActiveBlockAsync(idUsuario);
            if (!ok) return NotFound(new { message = "Nenhum bloqueio ativo encontrado." });
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Status(int idUsuario)
        {
            var rest = await _repo.GetActiveBlockRemainingDaysAsync(idUsuario);
            return Ok(new { ativo = rest.HasValue, restantesDias = rest ?? 0 });
        }
    }
}
