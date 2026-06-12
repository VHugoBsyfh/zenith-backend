using Backend.Data;
using Backend.DTOs.Rankings;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class RankingRepository : IRankingRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public RankingRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public async Task<List<RankingUsuarioItem>> GetTopAventureirosAsync(int take, int minMissoes, string? orderBy)
        {
            // Abordagem com Subconsultas (À prova de falhas no EF Core)
            var query = _ctx.Usuarios
                .Where(u => u.TipoUsuario == "Aventureiro")
                .Select(u => new 
                {
                    UsuarioId = u.Id,
                    Nome = u.Nome,
                    Classe = u.Classe,
                    Nivel = u.Nivel,
                    Reputacao = u.Reputacao,
                    
                    // EF Core traduz isso perfeitamente para sub-selects no SQL
                    Totais = _ctx.HistoricoMissoes.Count(h => h.IdUsuario == u.Id),
                    Concluidas = _ctx.HistoricoMissoes.Count(h => h.IdUsuario == u.Id && h.Resultado == "Concluída"),
                    UltimaAtividade = _ctx.HistoricoMissoes.Where(h => h.IdUsuario == u.Id).Max(h => (DateTime?)h.DataRegistro)
                })
                .Where(x => x.Totais >= minMissoes)
                .Select(x => new RankingUsuarioItem
                {
                    UsuarioId = x.UsuarioId,
                    Nome = x.Nome,
                    Classe = x.Classe,
                    Nivel = x.Nivel,
                    Reputacao = x.Reputacao,
                    MissoesTotais = x.Totais,
                    MissoesConcluidas = x.Concluidas,
                    TaxaSucesso = x.Totais == 0 ? 0m : (decimal)x.Concluidas / x.Totais,
                    UltimaAtividade = x.UltimaAtividade
                });

            // Ordenação Dinâmica
            query = orderBy?.ToLower() switch
            {
                "nivel" => query.OrderByDescending(x => x.Nivel).ThenByDescending(x => x.Reputacao),
                "concluidas" => query.OrderByDescending(x => x.MissoesConcluidas).ThenByDescending(x => x.Reputacao),
                _ => query.OrderByDescending(x => x.Reputacao).ThenByDescending(x => x.Nivel) // Padrão
            };

            return await query
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RankingGrupoItem>> GetTopGruposAsync(int take, int minMissoes)
        {
            // O mesmo padrão de Subconsulta para Grupos
            var query = _ctx.Grupos
                .Select(g => new
                {
                    GrupoId = g.Id,
                    NomeGrupo = g.NomeGrupo,
                    // Reputacao = g.Reputacao, // Descomente se quiser listar reputação do grupo
                    MembrosCount = _ctx.Usuarios.Count(u => u.IdGrupo == g.Id),
                    Totais = _ctx.MissoesAceitas.Count(ma => ma.IdGrupo == g.Id),
                    Concluidas = _ctx.MissoesAceitas.Count(ma => ma.IdGrupo == g.Id && ma.StatusMissao == "Concluída")
                })
                .Where(x => x.Totais >= minMissoes)
                .Select(x => new RankingGrupoItem
                {
                    GrupoId = x.GrupoId,
                    NomeGrupo = x.NomeGrupo,
                    Membros = x.MembrosCount,
                    MissoesTotais = x.Totais,
                    MissoesConcluidas = x.Concluidas,
                    TaxaSucesso = x.Totais == 0 ? 0m : (decimal)x.Concluidas / x.Totais
                });

            // Ordenação Padrão dos grupos
            return await query
                .OrderByDescending(x => x.TaxaSucesso)
                .ThenByDescending(x => x.MissoesConcluidas)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}