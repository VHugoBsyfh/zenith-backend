using Backend.Services; // Onde está o seu PagamentoService
using Backend.Repositories.Interfaces; // Onde está o seu IMissaoRepository
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/pagamentos")] // Nova rota principal para pagamentos
    public class PagamentosController : ControllerBase
    {
        private readonly IMissaoRepository _missoes;

        // Injetando o repositório de missões no construtor da Controller
        public PagamentosController(IMissaoRepository missoes)
        {
            _missoes = missoes;
        }

        [Authorize]
        [HttpPost("missoes/{idMissao:int}/gerar-pix")]
        public async Task<IActionResult> GerarCobrancaPix(int idMissao, [FromServices] PagamentoService pagamentos)
        {
            // 1. Busca a missão no banco
            var missao = await _missoes.GetByIdAsync(idMissao);
            
            if (missao == null)
                return NotFound(new { message = "Missão não encontrada." });

            if (missao.Status != "Concluída")
                return BadRequest(new { message = "A missão precisa ser concluída antes de ser paga." });

            // 2. Manda a API do Mercado Pago gerar a cobrança no valor exato da missão
            // Nota: Lembre-se de criar o arquivo PagamentoService.cs com aquele código que te mandei!
            var copiaECola = await pagamentos.GerarPixMercadoPagoAsync(
                missao.Recompensa, 
                "email.do.criador@teste.com", // Em um cenário real, você puxaria o e-mail do criador logado
                $"Recompensa Zenith: {missao.Titulo}"
            );

            // 3. Devolve para o front-end mostrar na tela!
            return Ok(new 
            { 
                message = "PIX gerado com sucesso!", 
                pixCopiaECola = copiaECola 
            });
        }
    }
}