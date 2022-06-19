using FoodstuffsRating.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodstuffsRating.Data.Dal
{
    public class BackendDbContext : DbContext
    {
        public BackendDbContext(DbContextOptions<BackendDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Email);

                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<UserRefreshToken>(entity =>
            {
                entity.ToTable("UserRefreshTokens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.RefreshToken).HasMaxLength(400);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId);
            });
        }

        [Obsolete("Use async version")]
        public override int SaveChanges()
        {
            this.OnBeforeSaving();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.OnBeforeSaving();

            return base.SaveChangesAsync(cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var utcNow = DateTime.UtcNow;
            var entries = this.ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is ITrackableCreationDate trackableCreationDate &&
                    entry.State == EntityState.Added)
                {
                    trackableCreationDate.CreatedAtUtc = utcNow;
                }

                if (entry.Entity is ITrackableDate trackableDate &&
                    (entry.State is EntityState.Added or EntityState.Modified))
                {
                    trackableDate.LastUpdatedAtUtc = utcNow;
                }
            }
        }
    }
}
