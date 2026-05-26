namespace Backend.DTOs.Contratos
{
    public class ContratoGenerateRequest
    {
        public string? Penalidades { get; set; }  // texto extra (opcional)
        public string? Recompensas { get; set; }  // texto extra (opcional)
        public string? Observacoes { get; set; }  // campo livre
    }
}
