using System;
using System.Threading;
using System.Threading.Tasks;
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
        public DbSet<UserExternalLogin> UserExternalLogins { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Email);

                entity.Property(x => x.Email).HasMaxLength(256);
                entity.Property(x => x.FirstName).HasMaxLength(50);
                entity.Property(x => x.LastName).HasMaxLength(50);
                entity.Property(x => x.PasswordHash).HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<UserRefreshToken>(entity =>
            {
                entity.ToTable("UserRefreshTokens");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();

                entity.HasIndex(x => x.UserId);

                entity.Property(x => x.RefreshToken).HasMaxLength(400);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.RefreshTokens)
                    .HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<UserExternalLogin>(entity =>
            {
                entity.ToTable("UserExternalLogins");
                entity.HasKey(x => new {x.LoginProvider, x.ExternalUserId});

                entity.Property(x => x.LoginProvider).HasMaxLength(50)
                    .HasConversion(x => x.ToString(), x => Enum.Parse<ExternalLoginProvider>(x));

                entity.Property(x => x.ExternalUserId).HasMaxLength(128);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.ExternalLogins)
                    .HasForeignKey(x => x.UserId);
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
