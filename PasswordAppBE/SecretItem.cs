using System.ComponentModel.DataAnnotations;

namespace PasswordApp
{
    public class SecretItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string? UserName { get; set; }

        public string PasswordHash { get; set; } = null!;

        public string? WebsiteUrl { get; set; }
        public string? Notes { get; set; }

        public int VaultId { get; set; }

        public virtual Vault Vault { get; set; } = null!;
    }
}
