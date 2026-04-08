using Microsoft.EntityFrameworkCore;
using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Infrastructure.Persistence
{
    /// <summary>
    /// Database Context - Infrastructure Layer
    /// Manages Entity Framework Core configuration and database operations
    /// This is part of the Frameworks & Drivers layer (external detail)
    /// </summary>
    public class TechAcademyDbContext : DbContext
    {
        public TechAcademyDbContext(DbContextOptions<TechAcademyDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Restaurant)
                .WithMany(res => res.Reviews)
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Review>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            SeedInitialData(modelBuilder);
        }

        private static void SeedInitialData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurant>().HasData(
                new Restaurant
                {
                    Id = 1,
                    Name = "The Italian Kitchen",
                    City = "New York",
                    IsActive = true,
                    CreatedDate = new DateTime(2025, 1, 15, 10, 30, 0)
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Dragon Palace",
                    City = "San Francisco",
                    IsActive = true,
                    CreatedDate = new DateTime(2025, 2, 20, 14, 45, 0)
                },
                new Restaurant
                {
                    Id = 3,
                    Name = "Le Petit Bistro",
                    City = "Boston",
                    IsActive = true,
                    CreatedDate = new DateTime(2025, 1, 10, 9, 15, 0)
                },
                new Restaurant
                {
                    Id = 4,
                    Name = "Sakura Sushi House",
                    City = "Seattle",
                    IsActive = true,
                    CreatedDate = new DateTime(2025, 3, 5, 11, 20, 0)
                },
                new Restaurant
                {
                    Id = 5,
                    Name = "The Mexican Grill",
                    City = "Austin",
                    IsActive = true,
                    CreatedDate = new DateTime(2025, 2, 28, 16, 0, 0)
                }
            );

            modelBuilder.Entity<Review>().HasData(
                new Review
                {
                    Id = 1,
                    RestaurantId = 1,
                    ReviewerName = "Alice Johnson",
                    Rating = 5,
                    Comment = "Excellent pasta and authentic Italian flavors! The ambiance is warm and inviting.",
                    CreatedDate = new DateTime(2025, 1, 20, 12, 30, 0)
                },
                new Review
                {
                    Id = 2,
                    RestaurantId = 1,
                    ReviewerName = "Bob Smith",
                    Rating = 4,
                    Comment = "Great food, but a bit pricey. Service was attentive and quick.",
                    CreatedDate = new DateTime(2025, 1, 25, 18, 45, 0)
                },
                new Review
                {
                    Id = 3,
                    RestaurantId = 2,
                    ReviewerName = "Charlie Brown",
                    Rating = 5,
                    Comment = "Amazing dim sum! Fresh ingredients and perfect cooking. Must visit.",
                    CreatedDate = new DateTime(2025, 2, 22, 19, 0, 0)
                },
                new Review
                {
                    Id = 4,
                    RestaurantId = 3,
                    ReviewerName = "Diana Prince",
                    Rating = 4,
                    Comment = "Charming atmosphere and delicious food. French classics done right.",
                    CreatedDate = new DateTime(2025, 1, 12, 20, 15, 0)
                },
                new Review
                {
                    Id = 5,
                    RestaurantId = 4,
                    ReviewerName = "Emma Davis",
                    Rating = 5,
                    Comment = "Best sushi in the city! Incredible quality and presentation. Highly recommend.",
                    CreatedDate = new DateTime(2025, 3, 8, 18, 30, 0)
                }
            );
        }
    }
}
