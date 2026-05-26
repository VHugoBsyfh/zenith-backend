using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Backend.DTOs.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/usuarios")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UsuarioAdminService _service;
        public AdminController(UsuarioAdminService service) => _service = service;

        // GET: lista paginada
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? q,
            [FromQuery] string? tipo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (total, items) = await _service.ListarAsync(q, tipo, page, pageSize);
            return Ok(new { total, page, pageSize, items });
        }

        // PUT: trocar perfil (Aventureiro/Criador/Admin)
        [HttpPut("{id:int}/perfil")]
        public async Task<IActionResult> TrocarPerfil(int id, [FromBody] UsuarioAdminUpdateRoleRequest req)
        {
            try
            {
                await _service.AtualizarTipoAsync(id, req.NovoTipoUsuario);
                return NoContent();
            }
            catch (ValidationException ex) { return UnprocessableEntity(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST: reset de senha
        [HttpPost("{id:int}/reset-senha")]
        public async Task<IActionResult> ResetSenha(int id, [FromBody] ResetSenhaRequest req)
        {
            try
            {
                await _service.ResetarSenhaAsync(id, req.NovaSenha);
                return NoContent();
            }
            catch (ValidationException ex) { return UnprocessableEntity(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
