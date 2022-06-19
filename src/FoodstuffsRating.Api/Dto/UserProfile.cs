namespace FoodstuffsRating.Api.Dto
{
    public class UserProfile
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
    }
}
