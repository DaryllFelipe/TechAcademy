using Microsoft.EntityFrameworkCore;
using TechAcademyApi.Core.Abstractions;
using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Restaurant Repository - Infrastructure Layer
    /// Concrete implementation for Restaurant-specific data operations
    /// </summary>
    public class RestaurantRepository : GenericRepository<Restaurant>, IRestaurantRepository
    {
        private readonly TechAcademyDbContext _dbContext;

        public RestaurantRepository(TechAcademyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Restaurant?> GetRestaurantWithReviewsAsync(int restaurantId)
        {
            return await _dbContext.Restaurants
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
        }

        public async Task<IEnumerable<Restaurant>> GetActiveRestaurantsAsync()
        {
            return await _dbContext.Restaurants
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Restaurant>> GetAllRestaurantsWithReviewsAsync()
        {
            return await _dbContext.Restaurants
                .Include(r => r.Reviews)
                .ToListAsync();
        }
    }
}
