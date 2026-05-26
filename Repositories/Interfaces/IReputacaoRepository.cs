using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IReputacaoRepository
    {
        Task<List<Avaliacao>> ListarAvaliacoesRecebidasAsync(int idUsuario, int max = 50);
        Task<decimal> SomatorioPenalidadesAsync(int idUsuario);
        Task AtualizarReputacaoAsync(int idUsuario, decimal reputacao);
    }
}
