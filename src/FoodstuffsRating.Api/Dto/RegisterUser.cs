using System.ComponentModel.DataAnnotations;

namespace FoodstuffsRating.Api.Dto
{
    public class RegisterUserRequest
    {
        [Required]
        [StringLength(256, MinimumLength = 5)]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}
