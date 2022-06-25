using System;
using System.ComponentModel.DataAnnotations;

namespace FoodstuffsRating.Dto
{
    public class UserProfile
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }

    public class UserProfileUpdateRequest
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }
    }
}
