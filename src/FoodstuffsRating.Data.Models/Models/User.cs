using System;
using System.Collections.Generic;

namespace FoodstuffsRating.Data.Models
{
    public sealed class User : ITrackableDate
    {
        public User()
        {
            this.RefreshTokens = new HashSet<UserRefreshToken>();
            this.ExternalLogins = new List<UserExternalLogin>();
        }

        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public bool IsEmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime? LastLoginDateUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }

        public ICollection<UserRefreshToken> RefreshTokens { get; set; }
        public ICollection<UserExternalLogin> ExternalLogins { get; set; }
    }
}
