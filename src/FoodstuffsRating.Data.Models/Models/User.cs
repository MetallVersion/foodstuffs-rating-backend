namespace FoodstuffsRating.Data.Models
{
    public class User : ITrackableDate
    {
        public User()
        {
            this.RefreshTokens = new HashSet<UserRefreshToken>();
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

        public virtual ICollection<UserRefreshToken> RefreshTokens { get; set; }
        
    }
}
