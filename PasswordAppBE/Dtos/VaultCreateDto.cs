using System.ComponentModel.DataAnnotations;

namespace PasswordApp.Dtos
{
    public class VaultCreateDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
