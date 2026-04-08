using MediatR;
using TechAcademyApi.Application.DTOs;

namespace TechAcademyApi.Application.Features.Restaurants.Queries
{
    /// <summary>
    /// Query to retrieve all restaurants
    /// Part of CQRS pattern - separates read operations from write operations
    /// </summary>
    public class GetAllRestaurantsQuery : IRequest<IEnumerable<RestaurantDto>>
    {
    }

    /// <summary>
    /// Query to retrieve a specific restaurant by ID
    /// </summary>
    public class GetRestaurantByIdQuery : IRequest<RestaurantDto?>
    {
        public int Id { get; set; }

        public GetRestaurantByIdQuery(int id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// Query to retrieve all restaurants with their related reviews
    /// Uses eager loading with EF Core Include()
    /// </summary>
    public class GetRestaurantsWithReviewsQuery : IRequest<IEnumerable<RestaurantWithReviewsDto>>
    {
    }
}
