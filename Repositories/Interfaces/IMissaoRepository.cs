using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IMissaoRepository
    {
        Task<Missao> AddAsync(Missao missao);
        Task<IEnumerable<Missao>> GetAllAsync();
        Task<Missao?> GetByIdAsync(int id);
        Task UpdateAsync(Missao missao);
        Task DeleteAsync(int id);
        Task<IEnumerable<Missao>> FiltrarAsync(
    string? tipo,
    string? localizacao,
    string? classe,
    int? nivelMaximo,
    decimal? recompensaMinima,
    int? idCriador,
    int? idAventureiro,
    int? idGrupo);
        Task<IEnumerable<Missao>> RecomendadasAsync(string classe, int nivel, int top);
        Task<Missao?> GetByIdForUpdateAsync(int id);
        Task<bool> HasVinculosAsync(int missaoId);
        Task SetStatusAsync(int idMissao, string novoStatus);
        Task DesvincularAventureiroAsync(int idMissao, string novoStatus);
        Task VincularAventureiroAsync(int idMissao, int idAventureiro, string novoStatus);
        Task VincularGrupoAsync(int idMissao, int idGrupo, string status);
    }
}
