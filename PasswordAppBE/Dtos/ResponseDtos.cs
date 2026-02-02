using System.ComponentModel.DataAnnotations;

namespace PasswordApp.Dtos
{
    public class ResponseDtos
    {
        // Bir kasayı dönerken kullanılacak model
        public class VaultDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }

        // Bir şifreyi dönerken kullanılacak model
        public class SecretItemDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string? UserName { get; set; }

            //Artık düz metin parola için bir alanımız var

            public string PasswordHash { get; set; } = null!; // Çözülmemiş

            public string? WebsiteUrl { get; set; }
            public string? Notes { get; set; }
        }
    }
}
