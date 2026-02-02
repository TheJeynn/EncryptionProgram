using System.ComponentModel.DataAnnotations;

namespace PasswordApp
{
    public class Vault
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string UserId { get; set; } = null!;

        public virtual AppUser User { get; set; } = null!;
    }
}
