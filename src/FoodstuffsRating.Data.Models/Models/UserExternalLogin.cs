using System;

namespace FoodstuffsRating.Data.Models
{
    public sealed class UserExternalLogin : ITrackableDate
    {
        public ExternalLoginProvider LoginProvider { get; set; }
        public string ExternalUserId { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }

        public User User { get; set; } = null!;
    }

    public enum ExternalLoginProvider
    {
        Google = 0,
        Facebook = 1
    }
}