using MediatR;
using TechAcademyApi.Application.DTOs;

namespace TechAcademyApi.Application.Features.Restaurants.Queries
{
    /// <summary>
    /// CQRS Query to retrieve total count of restaurants
    /// Returns the total number of active restaurants in the database
    /// </summary>
    public record GetTotalRestaurantsQuery : IRequest<TotalRestaurantsSummaryDto>;
}
