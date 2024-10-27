using System.ComponentModel.DataAnnotations;

namespace LidomNet.Data.DTOs
{
    public class UserRegistrationRequestDto
    {
        [Required]
        [MaxLength(255, ErrorMessage = "Name cant have more than 252 characters")]
        public string Name { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Email cant have more than 252 characters")]
        public string EmailAddress { get; set; }
        [Required]
        [MaxLength(255, ErrorMessage = "Password cant have more than 252 characters")]
        public string Password { get; set; }
    }
}
