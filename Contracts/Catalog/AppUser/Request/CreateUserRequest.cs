using System.ComponentModel.DataAnnotations;

namespace Contracts.Catalog.Request
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(60, ErrorMessage = "Name can't be longer than 60 characters")]
        public string Email { get; set; } = string.Empty;
        public string? CustomTag { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class CreateUserWithPasswordRequest : CreateUserRequest
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(60, ErrorMessage = "Password can't be longer than 60 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class CreateAdminUserRequest : CreateUserRequest
    {
        [Required]
        public int TenantId { get; set; }
    }

    public class CreateAdminUserWithPasswordRequest : CreateUserWithPasswordRequest
    {
        [Required]
        public int TenantId { get; set; }
    }
}
