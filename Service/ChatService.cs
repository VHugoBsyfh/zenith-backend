using Backend.DTOs;
using Backend.Models;
using Backend.Repositories.Interfaces;

namespace Backend.Services
{
    public class ChatService
    {
        private readonly IMensagemRepository _msgs;
        private readonly IGrupoRepository _grupos;
        private readonly IUsuarioRepository _usuarios;



        public ChatService(IMensagemRepository msgs, IGrupoRepository grupos, IUsuarioRepository usuarios)
        {
            _msgs = msgs;
            _grupos = grupos;
            _usuarios = usuarios;
        }

        public async Task<MessageResponse> EnviarAsync(int idGrupo, int autorId, SendMessageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Conteudo))
                throw new ArgumentException("Mensagem vazia.");

            // só membro do grupo pode enviar
            if (!await _grupos.IsMembroAsync(idGrupo, autorId))
                throw new UnauthorizedAccessException("Você não é membro deste grupo.");

            var msg = new Mensagem
            {
                IdGrupo = idGrupo,
                IdUsuario = autorId,
                MensagemTexto = req.Conteudo.Trim(),
                DataEnvio = DateTime.Now
            };

            var saved = await _msgs.EnviarAsync(msg);

            // para resposta, buscamos o nome via repositório de listagem (ou cache simples)
            var user = await _usuarios.GetByIdAsync(autorId);
            var autorNome = user?.Nome ?? "Desconhecido";

            return new MessageResponse
            {
                Id = saved.Id,
                IdGrupo = idGrupo,
                AutorId = autorId,
                AutorNome = autorNome,
                Conteudo = saved.MensagemTexto,
                DataEnvio = saved.DataEnvio
            };
        }

        public async Task<IEnumerable<MessageResponse>> ListarAsync(int idGrupo, int solicitanteId, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            // só membro pode ler
            if (!await _grupos.IsMembroAsync(idGrupo, solicitanteId))
                throw new UnauthorizedAccessException("Você não é membro deste grupo.");

            var skip = (page - 1) * pageSize;
            var items = await _msgs.ListarPorGrupoAsync(idGrupo, skip, pageSize);

            return items.Select(x => new MessageResponse
            {
                Id = x.msg.Id,
                IdGrupo = x.msg.IdGrupo,
                AutorId = x.msg.IdUsuario,
                AutorNome = x.autorNome,
                Conteudo = x.msg.MensagemTexto,
                DataEnvio = x.msg.DataEnvio
            });
        }
    }
}
