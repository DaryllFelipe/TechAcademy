using MediatR;
using TechAcademyApi.Application.DTOs;

namespace TechAcademyApi.Application.Features.Restaurants.Queries
{
    /// <summary>
    /// Query to retrieve all restaurants with combined external API data
    /// Fetches geolocation and weather information for each restaurant's city
    /// Part of CQRS pattern
    /// </summary>
    public class GetRestaurantsWithExternalDataQuery : IRequest<IEnumerable<RestaurantWithExternalDataDto>>
    {
    }
}
