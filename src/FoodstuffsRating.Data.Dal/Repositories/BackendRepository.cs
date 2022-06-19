namespace FoodstuffsRating.Data.Dal
{
    public interface IBackendRepository<T> : IRepositoryBase<T, BackendDbContext>
        where T : class
    {
    }

    public class BackendRepository<T> : RepositoryBase<T, BackendDbContext>, IBackendRepository<T>
        where T : class
    {
        public BackendRepository(BackendDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
