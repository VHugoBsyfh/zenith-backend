using Backend.DTOs.Notificacoes;
using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class NotificacaoService
    {
        private readonly INotificacaoRepository _repo;
        public NotificacaoService(INotificacaoRepository repo) => _repo = repo;

        public async Task<NotificacaoResponse> CriarAsync(NotificacaoCreateRequest req)
        {
            var ent = new Notificacao
            {
                IdUsuario = req.IdUsuario,
                Tipo = req.Tipo.Trim(),
                Titulo = req.Titulo.Trim(),
                Mensagem = req.Mensagem.Trim(),
                Link = string.IsNullOrWhiteSpace(req.Link) ? null : req.Link.Trim()
            };
            var saved = await _repo.CriarAsync(ent);
            return ToResponse(saved);
        }

        public async Task<(int totalNaoLidas, IEnumerable<NotificacaoResponse> items)> ListarAsync(
            int idUsuario, bool somenteNaoLidas, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
            var skip = (page - 1) * pageSize;

            var total = await _repo.ContarNaoLidasAsync(idUsuario);
            var list = await _repo.ListarAsync(idUsuario, somenteNaoLidas, pageSize, skip);
            return (total, list.Select(ToResponse));
        }

        public async Task<bool> MarcarComoLidaAsync(int id, int idUsuario)
        {
            var n = await _repo.GetByIdAsync(id, idUsuario);
            if (n == null) return false;
            if (!n.Lida)
                await _repo.MarcarComoLidaAsync(n);
            return true;
        }

        private static NotificacaoResponse ToResponse(Notificacao n) => new()
        {
            Id = n.Id,
            Tipo = n.Tipo,
            Titulo = n.Titulo,
            Mensagem = n.Mensagem,
            Link = n.Link,
            Lida = n.Lida,
            DataCriacao = n.DataCriacao
        };
    }
}
