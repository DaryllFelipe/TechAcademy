using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Core.Abstractions
{
    /// <summary>
    /// Review Repository Interface - Abstraction for review data persistence
    /// Extends the generic IRepository for specialized review operations
    /// Defined in Core layer following Dependency Inversion Principle
    /// </summary>
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByRestaurantAsync(int restaurantId);
        Task<double> GetAverageRatingAsync(int restaurantId);
    }
}
