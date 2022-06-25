using FoodstuffsRating.Data.Models;

namespace FoodstuffsRating.Models.Auth
{
    public class ExternalProviderRegistrationModel
    {
        public ExternalLoginProvider LoginProvider { get; set; }

        public string ExternalUserId { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;
    }
}
