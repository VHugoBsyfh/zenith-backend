namespace Backend.DTOs
{
    public class AvaliacaoGrupoCreateRequest
    {
        public int IdMissaoAceita { get; set; }
        public int IdGrupo { get; set; }
        public int Nota { get; set; }
        public string? Justificativa { get; set; }
    }
}