namespace Backend.DTOs
{
    public class GrupoResponse
    {
        public int Id { get; set; }
        public string NomeGrupo { get; set; } = null!;
        public int QuantidadeMembros { get; set; }
        public int? IdMissaoVinculada { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
