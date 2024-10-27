using System.ComponentModel.DataAnnotations;


namespace LidomNet.Data.DTOs
{
    public class UserLoginRequestDto
    {
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
