using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class GrupoCreateRequest
    {
        [Required, MaxLength(100)]
        public string NomeGrupo { get; set; } = null!;
    }
}
