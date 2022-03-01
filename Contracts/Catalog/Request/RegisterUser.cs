using System.ComponentModel.DataAnnotations;

namespace Contracts.Catalog.Request
{
    public class RegisterUser
    {
        [Required]
        [EmailAddress]
        [StringLength(64)]
        public string Email { get; set; } = string.Empty;
    }
}
