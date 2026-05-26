namespace Backend.DTOs.Recomendacoes
{
    public class MissaoRecomendadaResponse
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Localizacao { get; set; } = null!;
        public decimal Recompensa { get; set; }
        public string TipoMissao { get; set; } = null!;
        public int NivelMinimo { get; set; }
        public string? ClassePreferida { get; set; }
        public int IdCriador { get; set; }
        public string NomeCriador { get; set; } = null!;
        public decimal Score { get; set; }   // quanto maior, melhor
    }
}
