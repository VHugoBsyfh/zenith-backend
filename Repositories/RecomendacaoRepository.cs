using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class RecomendacaoRepository : IRecomendacaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public RecomendacaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;

        public Task<Usuario?> ObterUsuarioAsync(int idUsuario)
            => _ctx.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario)!;

        public async Task<Dictionary<string, int>> ContarTiposConcluidosAsync(int idUsuario, int topConsiderar = 3)
        {
            // Tipos de missão mais concluídos pelo usuário
            var query =
                from h in _ctx.HistoricoMissoes
                join ma in _ctx.MissoesAceitas on h.IdMissaoAceita equals ma.Id
                join m in _ctx.Missoes on ma.IdMissao equals m.Id
                where h.IdUsuario == idUsuario && h.Resultado == "Concluída"
                group m by m.TipoMissao into g
                orderby g.Count() descending
                select new { Tipo = g.Key, Qtde = g.Count() };

            return await query.Take(topConsiderar)
                              .ToDictionaryAsync(x => x.Tipo, x => x.Qtde);
        }
        public async Task<int> ObterMaiorNivelEquipeAsync(int idUsuario)
        {
            // 1. Pega os IDs dos grupos que o usuário faz parte
            var gruposDoUsuario = await _ctx.GrupoUsuarios
                .Where(gu => gu.IdUsuario == idUsuario)
                .Select(gu => gu.IdGrupo)
                .ToListAsync();

            if (!gruposDoUsuario.Any()) return 0; // Usuário não está em nenhum grupo

            // 2. Acha o maior nível de qualquer membro pertencente a esses grupos
            var maiorNivel = await _ctx.GrupoUsuarios
                .Where(gu => gruposDoUsuario.Contains(gu.IdGrupo))
                .Join(_ctx.Usuarios, gu => gu.IdUsuario, u => u.Id, (gu, u) => u.Nivel)
                .MaxAsync();

            return maiorNivel;
        }

        // public async Task<List<(Missao m, Usuario criador)>> ListarMissoesDisponiveisAsync(string? localizacao = null)
        // {
        //     var q =
        //         from m in _ctx.Missoes
        //         join u in _ctx.Usuarios on m.IdCriador equals u.Id
        //         where m.Status == "Disponível"
        //         select new { m, u };

        //     if (!string.IsNullOrWhiteSpace(localizacao))
        //         q = q.Where(x => x.m.Localizacao == localizacao);

        //     var list = await q.AsNoTracking()
        //                       .OrderByDescending(x => x.m.DataCriacao)
        //                       .ToListAsync();

        //     return list.Select(x => (x.m, x.u)).ToList();
        // }
        //
        public async Task<List<(Missao m, Usuario criador)>> ListarMissoesDisponiveisScoringAsync(int idUsuario, string? localizacao = null)
        {
            var q = from m in _ctx.Missoes
                    join u in _ctx.Usuarios on m.IdCriador equals u.Id
                    where m.Status == "Disponível" && m.IdCriador != idUsuario
                    select new { m, u };

            if (!string.IsNullOrWhiteSpace(localizacao))
                q = q.Where(x => x.m.Localizacao == localizacao);

            var list = await q.AsNoTracking().ToListAsync();
            return list.Select(x => (x.m, x.u)).ToList();
        }
    }
}
