using MediatR;
using TechAcademyApi.Application.DTOs;

namespace TechAcademyApi.Application.Features.Restaurants.Queries
{
    /// <summary>
    /// CQRS Query to retrieve review count per restaurant
    /// Returns list of restaurants with their review counts
    /// Sorted by restaurant name
    /// </summary>
    public record GetReviewsPerRestaurantQuery : IRequest<IEnumerable<RestaurantReviewCountDto>>;
}
