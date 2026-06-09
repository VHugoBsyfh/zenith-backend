using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class MissaoRepository : IMissaoRepository
    {
        private readonly GuildaDigitalContext _ctx;
        public MissaoRepository(GuildaDigitalContext ctx) => _ctx = ctx;


        public async Task<Missao> AddAsync(Missao missao)
        {
            _ctx.Missoes.Add(missao);
            await _ctx.SaveChangesAsync();
            return missao;
        }

        public async Task<IEnumerable<Missao>> GetAllAsync()
            => await _ctx.Missoes.AsNoTracking().ToListAsync();

        public async Task<Missao?> GetByIdAsync(int id)
            => await _ctx.Missoes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        public async Task UpdateAsync(Missao missao)
        {
            _ctx.Missoes.Update(missao);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var missao = await _ctx.Missoes.FindAsync(id);
            if (missao != null)
            {
                _ctx.Missoes.Remove(missao);
                await _ctx.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Missao>> FiltrarAsync(
    string? tipo,
    string? localizacao,
    string? classe,
    int? nivelMaximo,
    decimal? recompensaMinima,
    int? idCriador,
    int? idAventureiro,
    int? idGrupo)
        {
            var query = _ctx.Missoes.AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(m => m.TipoMissao.Contains(tipo));

            if (!string.IsNullOrEmpty(localizacao))
                query = query.Where(m => m.Localizacao.Contains(localizacao));

            if (!string.IsNullOrEmpty(classe))
                query = query.Where(m =>
                    m.ClassePreferida != null &&
                    m.ClassePreferida.Contains(classe));

            if (nivelMaximo.HasValue)
                query = query.Where(m => m.NivelMinimo <= nivelMaximo.Value);

            if (recompensaMinima.HasValue)
                query = query.Where(m => m.Recompensa >= recompensaMinima.Value);

            // Filtro por criador
            if (idCriador.HasValue)
                query = query.Where(m => m.IdCriador == idCriador.Value);

            // Filtro por aventureiro
            // Filtro por aventureiro (Ajustado para buscar pelo histórico de aceitação)
            if (idAventureiro.HasValue)
            {
                // 1. Buscamos todos os IDs de missões que este aventureiro aceitou um dia (ativo, concluído ou cancelado)
                var idsMissoesDoAventureiro = await _ctx.MissoesAceitas
                    .Where(ma => ma.IdUsuario == idAventureiro.Value)
                    .Select(ma => ma.IdMissao)
                    .ToListAsync();

                // 2. Filtramos a query principal para trazer essas missões
                query = query.Where(m => idsMissoesDoAventureiro.Contains(m.Id));
            }
            if (idGrupo.HasValue)
            {
                // 1. Buscamos todos os IDs de missões que este grupo aceitou na história
                var idsMissoesDoGrupo = await _ctx.MissoesAceitas
                    .Where(ma => ma.IdGrupo == idGrupo.Value)
                    .Select(ma => ma.IdMissao)
                    .ToListAsync();

                // 2. Filtramos a query principal para trazer apenas essas missões
                query = query.Where(m => idsMissoesDoGrupo.Contains(m.Id));
            }

            return await query.AsNoTracking().ToListAsync();
        }
        public async Task<IEnumerable<Missao>> RecomendadasAsync(string classe, int nivel, int top)
        {
            var query = _ctx.Missoes.AsNoTracking()
                .Where(m => m.Status == "Disponível" && m.NivelMinimo <= nivel);

            var ranked = query
                .Select(m => new
                {
                    Missao = m,
                    Score =
                        (m.ClassePreferida != null && m.ClassePreferida == classe ? 30 : 0) +
                        ((nivel - m.NivelMinimo) * 2) +
                        (int)(m.Recompensa / 100)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Missao.Recompensa)
                .Take(top);

            return (await ranked.ToListAsync()).Select(x => x.Missao);
        }
        public async Task<Missao?> GetByIdForUpdateAsync(int id)
    => await _ctx.Missoes.FirstOrDefaultAsync(m => m.Id == id);
        public async Task<bool> HasVinculosAsync(int missaoId)
        => await _ctx.MissoesAceitas.AsNoTracking().AnyAsync(x => x.IdMissao == missaoId);

        public async Task SetStatusAsync(int idMissao, string novoStatus)
        {
            var m = await _ctx.Missoes.FirstOrDefaultAsync(x => x.Id == idMissao);
            if (m != null)
            {
                m.Status = novoStatus;
                await _ctx.SaveChangesAsync();
            }
        }
        public async Task DesvincularAventureiroAsync(int idMissao, string novoStatus)
        {
            var m = await _ctx.Missoes.FirstOrDefaultAsync(x => x.Id == idMissao);
            if (m != null)
            {
                m.IdAventureiro = null; // A mágica reversa acontece aqui!
                m.Status = novoStatus;
                await _ctx.SaveChangesAsync();
            }
        }
        public async Task VincularAventureiroAsync(int idMissao, int idAventureiro, string novoStatus)
        {
            var m = await _ctx.Missoes.FirstOrDefaultAsync(x => x.Id == idMissao);
            if (m != null)
            {
                m.IdAventureiro = idAventureiro;
                m.Status = novoStatus;
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
