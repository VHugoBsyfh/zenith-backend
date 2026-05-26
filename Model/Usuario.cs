using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nome { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Senha { get; set; } = null!; // armazenar hash

        [Required, MaxLength(50)]
        public string Classe { get; set; } = null!; // Guerreiro, Mago...

        public int Nivel { get; set; } = 1;

        [Required, MaxLength(20)]
        public string TipoUsuario { get; set; } = null!; // Aventureiro/Criador

        [Column(TypeName = "decimal(5,2)")]
        public decimal Reputacao { get; set; } = 0.0M;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}
