using System;

namespace FoodstuffsRating.Data.Models
{
    public sealed class UserRefreshToken : ITrackableDate
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set;} = null!;
        public bool IsActive { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }

        public User User { get; set; } = null!;
    }
}