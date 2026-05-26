using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IMensagemRepository
    {
        Task<Mensagem> EnviarAsync(Mensagem msg);
        Task<List<(Mensagem msg, string autorNome)>> ListarPorGrupoAsync(int idGrupo, int skip, int take);
    }
}
