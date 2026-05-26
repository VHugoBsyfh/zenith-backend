namespace Backend.DTOs
{
    public class MissaoCreateRequest
    {
        public string Titulo { get; set; } = null!;
        public string Descricao { get; set; } = null!;
        public string Localizacao { get; set; } = null!;
        public decimal Recompensa { get; set; }
        public string TipoMissao { get; set; } = null!;
        public int NivelMinimo { get; set; }
        public string? ClassePreferida { get; set; }
        //public int IdCriador { get; set; }
    }
}
