using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IMissaoAceitaRepository
{
    Task<bool> UsuarioTemAtivaAsync(int idUsuario);
    Task<bool> GrupoTemAtivaAsync(int idGrupo);
    Task<bool> MissaoJaFoiAceitaAsync(int idMissao);
    Task<MissaoAceita> CriarAsync(MissaoAceita registro);
    Task<bool> ExisteEmAndamentoParaUsuarioAsync(int idUsuario);
    Task<bool> ExisteEmAndamentoParaAlgumMembroDoGrupoAsync(int idGrupo);
    Task<MissaoAceita?> ObterRegistroGrupoAsync(int idMissao, int idGrupo);
    Task AtualizarStatusRegistroAsync(int idRegistro, string novoStatus);

}


