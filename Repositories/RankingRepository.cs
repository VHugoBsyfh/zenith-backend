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

        public async Task<List<RankingUsuarioItem>> GetTopAventureirosAsync(int take, int minMissoes)
        {
            // Base em HistoricoMissoes (por usuário)
            var query =
                from u in _ctx.Usuarios
                where u.TipoUsuario == "Aventureiro"
                join h in _ctx.HistoricoMissoes on u.Id equals h.IdUsuario
                group h by new { u.Id, u.Nome, u.Classe, u.Nivel, u.Reputacao } into g
                let concluidas = g.Count(x => x.Resultado == "Concluída")
                let totais = g.Count()
                let taxa = (decimal)concluidas / (totais == 0 ? 1 : totais) // ou filtre depois
                let ultima = g.Max(x => x.DataRegistro)
                where totais >= minMissoes
                select new RankingUsuarioItem
                {
                    UsuarioId = g.Key.Id,
                    Nome = g.Key.Nome,
                    Classe = g.Key.Classe,
                    Nivel = g.Key.Nivel,
                    Reputacao = g.Key.Reputacao,
                    MissoesConcluidas = concluidas,
                    MissoesTotais = totais,
                    TaxaSucesso = taxa,
                    UltimaAtividade = ultima
                };


            // Ordenação: reputação desc, taxa desc, concluidas desc, última atividade desc
            return await query
                .OrderByDescending(x => x.Reputacao)
                .ThenByDescending(x => x.TaxaSucesso)
                .ThenByDescending(x => x.MissoesConcluidas)
                .ThenByDescending(x => x.UltimaAtividade)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RankingGrupoItem>> GetTopGruposAsync(int take, int minMissoes)
        {
            // Base em MissoesAceitas por grupo
            var qBase =
                from g in _ctx.Grupos
                join gu in _ctx.GrupoUsuarios on g.Id equals gu.IdGrupo into membros
                from m in membros.DefaultIfEmpty()
                join ma in _ctx.MissoesAceitas on g.Id equals ma.IdGrupo into accs
                from a in accs.DefaultIfEmpty()
                select new { Grupo = g, Membro = m, Aceite = a };

            var query =
                from x in qBase
                group x by new { x.Grupo.Id, x.Grupo.NomeGrupo } into gg
                let membrosCount = gg.Select(v => v.Membro).Where(v => v != null).Select(v => v!.IdUsuario).Distinct().Count()
                let concluidas = gg.Select(v => v.Aceite).Where(v => v != null && v!.StatusMissao == "Concluída").Count()
                let totais = gg.Select(v => v.Aceite).Where(v => v != null).Count()
                let taxa = totais == 0 ? 0m : (decimal)concluidas / totais
                let ultima = gg.Select(v => v.Aceite).Where(v => v != null).Max(v => v!.DataConclusao)
                where totais >= minMissoes
                select new RankingGrupoItem
                {
                    GrupoId = gg.Key.Id,
                    NomeGrupo = gg.Key.NomeGrupo,
                    Membros = membrosCount,
                    MissoesConcluidas = concluidas,
                    MissoesTotais = totais,
                    TaxaSucesso = taxa,
                    UltimaAtividade = ultima
                };

            // Ordenação: taxa desc, concluidas desc, membros desc, última atividade desc
            return await query
                .OrderByDescending(x => x.TaxaSucesso)
                .ThenByDescending(x => x.MissoesConcluidas)
                .ThenByDescending(x => x.Membros)
                .ThenByDescending(x => x.UltimaAtividade)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
