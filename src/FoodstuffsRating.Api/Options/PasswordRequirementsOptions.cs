using Microsoft.AspNetCore.Identity;

namespace FoodstuffsRating.Api.Options
{
    public class PasswordRequirementsOptions : PasswordOptions
    {
        public int MaxLength { get; set; } = 30;
    }
}
