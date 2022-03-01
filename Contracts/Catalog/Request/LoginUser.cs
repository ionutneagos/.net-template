using System.ComponentModel.DataAnnotations;

namespace Contracts.Catalog.Request
{
    public class LoginUser
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
