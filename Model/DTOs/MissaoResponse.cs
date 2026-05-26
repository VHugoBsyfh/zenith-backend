namespace Backend.DTOs
{
    public class MissaoResponse
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string TipoMissao { get; set; } = null!;
        public string Localizacao { get; set; } = null!;
        public decimal Recompensa { get; set; }
        public string Status { get; set; } = null!;
        public int NivelMinimo { get; set; }
        public string? ClassePreferida { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
