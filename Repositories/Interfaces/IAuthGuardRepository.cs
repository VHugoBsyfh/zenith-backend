namespace Backend.Repositories.Interfaces
{
    public interface IAuthGuardRepository
    {
        Task<bool> HasActiveBlockAsync(int idUsuario);
        Task<int?> GetActiveBlockRemainingDaysAsync(int idUsuario);
        Task ApplyBlockAsync(int idUsuario, int dias, string? motivo = null);
        Task<bool> RemoveActiveBlockAsync(int idUsuario); // encerra bloqueios ativos
    }
}
