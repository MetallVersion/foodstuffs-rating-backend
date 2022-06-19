namespace FoodstuffsRating.Data.Models
{
    public interface ITrackableCreationDate
    {
        DateTime CreatedAtUtc { get; set; }
    }

    public interface ITrackableDate : ITrackableCreationDate
    {
        DateTime LastUpdatedAtUtc { get; set; }
    }
}
