using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ConclusaoRepository : IConclusaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public ConclusaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<MissaoAceita?> GetAtivaAsync(int idMissao)
            => await _ctx.MissoesAceitas.FirstOrDefaultAsync(x => x.IdMissao == idMissao && x.StatusMissao == "Em andamento");

        public async Task AtualizarStatusAsync(MissaoAceita missao, string novoStatus)
        {
            missao.StatusMissao = novoStatus;
            missao.DataConclusao = DateTime.Now;
            await _ctx.SaveChangesAsync();
        }

        public async Task RegistrarHistoricoAsync(HistoricoMissao historico)
        {
            _ctx.HistoricoMissoes.Add(historico);
            await _ctx.SaveChangesAsync();
        }

        // ▼ NOVO: Adiciona a reputação e trava no máximo de 100
        public async Task AjustarReputacaoAsync(int idUsuario, decimal delta)
        {
            var usuario = await _ctx.Usuarios.FindAsync(idUsuario);
            if (usuario != null)
            {
                usuario.Reputacao += delta;
                
                // Se passar de 100, crava no 100 para não estourar o limite
                if (usuario.Reputacao > 100m)
                {
                    usuario.Reputacao = 100m;
                }
                
                await _ctx.SaveChangesAsync();
            }
        }

        // ▼ NOVO: Pega todos os IDs dos membros de um grupo específico
        // ▼ TEMPORÁRIO: Retorna uma lista vazia para não quebrar a compilação
        public async Task<List<int>> ListarMembrosDoGrupoAsync(int idGrupo)
        {
            // OBS: O DbContext ainda não tem a tabela de membros do grupo mapeada.
            // Quando a tabela de grupos estiver pronta, você pode voltar aqui e descomentar o código real.
            
            /* CÓDIGO FUTURO:
            return await _ctx.NOME_DA_SUA_TABELA_AQUI
                .Where(m => m.IdGrupo == idGrupo)
                .Select(m => m.IdUsuario)
                .ToListAsync();
            */

            return await Task.FromResult(new List<int>()); // Retorna vazio por enquanto
        }
    }
}