using System.ComponentModel.DataAnnotations;

namespace ŞifrelemeApp.Dtos
{
    public class SecretItemCreateDto
    {
        [Required]
        public string Title { get; set; } = null!; // Örn: "Facebook Hesabım"

        public string? UserName { get; set; } // Örn: "kullanici@adim.com"

        [Required]
        public string PasswordHash { get; set; } = null!; // Kullanıcının düz metin şifresi

        public string? WebsiteUrl { get; set; }
        public string? Notes { get; set; }
    }
}
