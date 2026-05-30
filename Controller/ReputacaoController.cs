using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReputacaoController : ControllerBase
    {
        private readonly ReputacaoService _service;
        public ReputacaoController(ReputacaoService service) => _service = service;

        [Authorize]
        [HttpPost("{idUsuario:int}/recalcular")]
        public async Task<IActionResult> Recalcular(int idUsuario)
        {
            // O resultado agora é uma tupla com as propriedades Reputacao e BloqueioDias
            var resultado = await _service.RecalcularAsync(idUsuario);
            
            return Ok(new 
            { 
                usuarioId = idUsuario, 
                reputacao = resultado.Reputacao,
                bloqueioDias = resultado.BloqueioDias // Mapeado para o JSON!
            });
        }
    }
}