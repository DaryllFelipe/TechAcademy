using MediatR;
using Microsoft.Extensions.Logging;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Queries;
using TechAcademyApi.Core.Abstractions;

namespace TechAcademyApi.Application.Features.Restaurants.Handlers
{
    /// <summary>
    /// Handler for GetReviewsPerRestaurantQuery
    /// Retrieves all restaurants with their review counts
    /// Uses LINQ to aggregate review data by restaurant
    /// </summary>
    public class GetReviewsPerRestaurantQueryHandler : IRequestHandler<GetReviewsPerRestaurantQuery, IEnumerable<RestaurantReviewCountDto>>
    {
        private readonly IRestaurantRepository _repository;
        private readonly ILogger<GetReviewsPerRestaurantQueryHandler> _logger;

        public GetReviewsPerRestaurantQueryHandler(
            IRestaurantRepository repository,
            ILogger<GetReviewsPerRestaurantQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RestaurantReviewCountDto>> Handle(
            GetReviewsPerRestaurantQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching review count per restaurant.");
            
            try
            {
                var restaurants = await _repository.GetAllRestaurantsWithReviewsAsync();

                var reviewCounts = restaurants
                    .Where(r => r.IsActive)
                    .Select(r => new RestaurantReviewCountDto
                    {
                        RestaurantId = r.Id,
                        RestaurantName = r.Name,
                        ReviewCount = r.Reviews?.Count ?? 0
                    })
                    .OrderBy(r => r.RestaurantName)
                    .ToList();

                _logger.LogInformation("Retrieved review count for {RestaurantCount} restaurants.", reviewCounts.Count);

                return reviewCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching review count per restaurant.");
                throw;
            }
        }
    }
}
