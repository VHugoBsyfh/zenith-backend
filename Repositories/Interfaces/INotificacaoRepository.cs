using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface INotificacaoRepository
    {
        Task<Notificacao> CriarAsync(Notificacao n);
        Task<List<Notificacao>> ListarAsync(int idUsuario, bool somenteNaoLidas, int take, int skip);
        Task<int> ContarNaoLidasAsync(int idUsuario);
        Task<Notificacao?> GetByIdAsync(int id, int idUsuario);
        Task MarcarComoLidaAsync(Notificacao n);
    }
}
