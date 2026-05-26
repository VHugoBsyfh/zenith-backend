namespace Backend.DTOs
{
    public class AddMembroRequest
    {
        // escolha 1: por Id
        public int IdUsuario { get; set; }

        // (opcional) escolha 2: por email, se quiser suportar
        public string? Email { get; set; }
    }
}
