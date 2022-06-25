using System.ComponentModel.DataAnnotations;

namespace FoodstuffsRating.Dto
{
    public class UserRegistrationRequest
    {
        [Required]
        [StringLength(256, MinimumLength = 5)]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }
    }

    public class UserRegistrationFromExternalRequest
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }
    }
}
