using MediatR;
using Microsoft.Extensions.Logging;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Queries;
using TechAcademyApi.Core.Abstractions;

namespace TechAcademyApi.Application.Features.Restaurants.Handlers
{
    /// <summary>
    /// Handler for GetMostRecentReviewQuery
    /// Retrieves the most recently created review with restaurant context
    /// Uses LINQ to find review with latest CreatedDate
    /// </summary>
    public class GetMostRecentReviewQueryHandler : IRequestHandler<GetMostRecentReviewQuery, MostRecentReviewDto?>
    {
        private readonly IRestaurantRepository _repository;
        private readonly ILogger<GetMostRecentReviewQueryHandler> _logger;

        public GetMostRecentReviewQueryHandler(
            IRestaurantRepository repository,
            ILogger<GetMostRecentReviewQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MostRecentReviewDto?> Handle(
            GetMostRecentReviewQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching most recent review.");
            
            try
            {
                var restaurants = await _repository.GetAllRestaurantsWithReviewsAsync();

                // Flatten all reviews from all restaurants and get the most recent one
                var mostRecentReview = restaurants
                    .Where(r => r.IsActive && r.Reviews != null && r.Reviews.Any())
                    .SelectMany(r => r.Reviews!.Select(rev => new { Restaurant = r, Review = rev }))
                    .OrderByDescending(x => x.Review.CreatedDate)
                    .FirstOrDefault();

                if (mostRecentReview == null)
                {
                    _logger.LogInformation("No reviews found in the system.");
                    return null;
                }

                var result = new MostRecentReviewDto
                {
                    ReviewId = mostRecentReview.Review.Id,
                    RestaurantId = mostRecentReview.Review.RestaurantId,
                    RestaurantName = mostRecentReview.Restaurant.Name,
                    ReviewerName = mostRecentReview.Review.ReviewerName,
                    Rating = mostRecentReview.Review.Rating,
                    Comment = mostRecentReview.Review.Comment,
                    CreatedDate = mostRecentReview.Review.CreatedDate
                };

                _logger.LogInformation("Most recent review found: ID={ReviewId}, Restaurant='{RestaurantName}', CreatedDate={CreatedDate}",
                    result.ReviewId, result.RestaurantName, result.CreatedDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching most recent review.");
                throw;
            }
        }
    }
}
