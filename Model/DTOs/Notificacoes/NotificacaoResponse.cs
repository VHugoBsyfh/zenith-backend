namespace Backend.DTOs.Notificacoes
{
    public class NotificacaoResponse
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string Mensagem { get; set; } = null!;
        public string? Link { get; set; }
        public bool Lida { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
