using Microsoft.EntityFrameworkCore;
using TechAcademyApi.Core.Abstractions;
using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Review Repository - Infrastructure Layer
    /// Concrete implementation for Review-specific data operations
    /// </summary>
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly TechAcademyDbContext _dbContext;

        public ReviewRepository(TechAcademyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<Review>> GetReviewsByRestaurantAsync(int restaurantId)
        {
            return await _dbContext.Reviews
                .Where(r => r.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int restaurantId)
        {
            var reviews = await _dbContext.Reviews
                .Where(r => r.RestaurantId == restaurantId)
                .ToListAsync();

            if (reviews.Count == 0)
                return 0;

            return reviews.Average(r => r.Rating);
        }
    }
}
