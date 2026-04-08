using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Core.Abstractions
{
    /// <summary>
    /// Restaurant Repository Interface - Abstraction for restaurant data persistence
    /// Extends the generic IRepository for specialized restaurant operations
    /// Defined in Core layer following Dependency Inversion Principle
    /// </summary>
    public interface IRestaurantRepository : IRepository<Restaurant>
    {
        Task<Restaurant?> GetRestaurantWithReviewsAsync(int restaurantId);
        Task<IEnumerable<Restaurant>> GetActiveRestaurantsAsync();
        Task<IEnumerable<Restaurant>> GetAllRestaurantsWithReviewsAsync();
    }
}
