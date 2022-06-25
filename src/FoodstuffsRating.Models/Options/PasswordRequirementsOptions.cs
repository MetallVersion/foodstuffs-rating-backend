using Microsoft.AspNetCore.Identity;

namespace FoodstuffsRating.Models.Options
{
    public class PasswordRequirementsOptions : PasswordOptions
    {
        public int MaxLength { get; set; } = 30;
    }
}
