using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

// NOTE: to use LinqKit method AsExpandable with async - we should upgrade version to LinqKit.Microsoft.EntityFrameworkCore which depends on net6.0
// so that's why this method is SYNC right now.
// AsExpandable allows you to use expressions with LINQ to SQL in EF
//using LinqKit;

namespace FoodstuffsRating.Data.Dal
{
    public interface IRepositoryBase<T, TContext>
        where T : class
        where TContext : DbContext
    {
        Task<T?> GetAsync(Expression<Func<T, bool>> where, bool asNoTracking = true,
            params Expression<Func<T, object>>[] includes);
        /// <summary>
        /// Return resource directly from storage (not from EF cache)
        /// </summary>
        Task<T?> GetFreshAsync(Expression<Func<T, bool>> where, bool asNoTracking = true);
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<List<T>> GetAllAsync(bool asNoTracking = true);
        Task<List<T>> GetManyAsync(Expression<Func<T, bool>> where,
            bool asNoTracking = true, params Expression<Func<T, object>>[]? includes);
        Task<List<TProjection>> GetManyAsProjectionAsync<TProjection>(Expression<Func<T, bool>> where,
            Expression<Func<T, TProjection>> projection);
        Task<int> CountAsync(Expression<Func<T, bool>> where);
        Task<long> LongCountAsync(Expression<Func<T, bool>> where);
        Task<bool> AnyAsync(Expression<Func<T, bool>> where);
        Task<List<T>> GetManyFromSqlRawAsync(string sql, bool asNoTracking = true, params object[] parameters);
        Task<T?> GetFromSqlRawAsync(string sql, bool asNoTracking = true, params object[] parameters);

        /// <summary>
        /// Set state of entity to Detached.
        /// It can be helpful if you need to retrieve fresh entity (directly from storage but not from EF cache)
        /// </summary>
        T DetachEntity(T existingEntity);
        Task AddAsync(T entity, bool commitChanges = true);
        Task UpdateAsync(T entity, bool commitChanges = true);
        Task RemoveAsync(T entity, bool commitChanges = true);
        Task RemoveAsync(IEnumerable<T> entities, bool commitChanges = true);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class RepositoryBase<T, TContext> : IRepositoryBase<T, TContext>
        where T : class
        where TContext : DbContext
    {
        protected readonly TContext DbContext;
        protected readonly DbSet<T> DbSet;

        public RepositoryBase(TContext dbContext)
        {
            this.DbContext = dbContext;
            this.DbSet = this.DbContext.Set<T>();
        }

        #region Read operations

        public virtual Task<T?> GetAsync(Expression<Func<T, bool>> where, bool asNoTracking = true,
            params Expression<Func<T, object>>[] includes)
        {
            var query = this.DbSet.Where(where);

            if (includes != null)
            {
                foreach (var expression in includes)
                {
                    query = query.Include(expression);
                }
            }

            return asNoTracking
                ? query.AsNoTracking().FirstOrDefaultAsync()
                : query.FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<T?> GetFreshAsync(Expression<Func<T, bool>> where, bool asNoTracking = true)
        {
            var entity = await this.GetAsync(@where, asNoTracking);
            if (entity == null)
            {
                return null;
            }

            this.DbContext.Entry(entity).State = EntityState.Detached;

            return await this.GetAsync(@where, asNoTracking);
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await this.DbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await this.DbSet.FindAsync(id);
        }

        public virtual Task<List<T>> GetAllAsync(bool asNoTracking = true)
        {
            return asNoTracking
                ? this.DbSet.AsNoTracking().ToListAsync()
                : this.DbSet.ToListAsync();
        }

        public virtual Task<List<T>> GetManyAsync(Expression<Func<T, bool>> where,
            bool asNoTracking = true, params Expression<Func<T, object>>[]? includes)
        {
            var query = asNoTracking
                ? this.DbSet.AsNoTracking()
                : this.DbSet;

            query = query.Where(where);

            if (includes != null)
            {
                foreach (var expression in includes)
                {
                    query = query.Include(expression);
                }
            }

            return query.ToListAsync();
        }

        public virtual Task<List<TProjection>> GetManyAsProjectionAsync<TProjection>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, TProjection>> projection)
        {
            var query = this.DbSet
                .Where(where)
                .AsNoTracking()
                .Select(projection);

            return query.ToListAsync();
        }

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> where)
        {
            return this.DbSet.CountAsync(where);
        }

        public virtual Task<long> LongCountAsync(Expression<Func<T, bool>> where)
        {
            return this.DbSet.LongCountAsync(where);
        }

        public virtual Task<bool> AnyAsync(Expression<Func<T, bool>> where)
        {
            return this.DbSet.AnyAsync(where);
        }

        /// <inheritdoc/>
        public virtual T DetachEntity(T existingEntity)
        {
            this.DbContext.Entry(existingEntity).State = EntityState.Detached;

            return existingEntity;
        }

        public virtual Task<List<T>> GetManyFromSqlRawAsync(string sql, bool asNoTracking = true,
            params object[] parameters)
        {
            var query = this.DbSet.FromSqlRaw(sql, parameters);
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query.ToListAsync();
        }

        public virtual Task<T?> GetFromSqlRawAsync(string sql, bool asNoTracking = true,
            params object[] parameters)
        {
            var query = this.DbSet.FromSqlRaw(sql, parameters);
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query.FirstOrDefaultAsync();
        }

        #endregion

        #region Create/Delete/Update operations

        public virtual Task AddAsync(T entity, bool commitChanges = true)
        {
            this.DbSet.Add(entity);

            if (commitChanges)
            {
                return this.SaveChangesAsync();
            }

            return Task.FromResult<object>(null!);
        }

        public virtual Task UpdateAsync(T entity, bool commitChanges = true)
        {
            this.DbSet.Update(entity);

            if (commitChanges)
            {
                return this.SaveChangesAsync();
            }

            return Task.FromResult<object>(null!);
        }

        public virtual Task RemoveAsync(T entity, bool commitChanges = true)
        {
            this.DbSet.Remove(entity);

            if (commitChanges)
            {
                return this.SaveChangesAsync();
            }

            return Task.FromResult<object>(null!);
        }

        public virtual Task RemoveAsync(IEnumerable<T> entities, bool commitChanges = true)
        {
            this.DbSet.RemoveRange(entities);

            if (commitChanges)
            {
                return this.SaveChangesAsync();
            }

            return Task.FromResult<object>(null!);
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await this.DbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion
    }
}
