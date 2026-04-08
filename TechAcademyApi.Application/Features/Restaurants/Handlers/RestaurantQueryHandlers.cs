using MediatR;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Queries;
using TechAcademyApi.Core.Abstractions;
using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Application.Features.Restaurants.Handlers
{
    /// <summary>
    /// Handler for GetAllRestaurantsQuery
    /// Retrieves all restaurants from the repository
    /// </summary>
    public class GetAllRestaurantsQueryHandler : IRequestHandler<GetAllRestaurantsQuery, IEnumerable<RestaurantDto>>
    {
        private readonly IRestaurantRepository _repository;

        public GetAllRestaurantsQueryHandler(IRestaurantRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<RestaurantDto>> Handle(GetAllRestaurantsQuery request, CancellationToken cancellationToken)
        {
            var restaurants = await _repository.GetAllAsync();
            return restaurants.Select(MapToDto).ToList();
        }

        private static RestaurantDto MapToDto(Restaurant restaurant)
        {
            return new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                City = restaurant.City,
                IsActive = restaurant.IsActive,
                CreatedDate = restaurant.CreatedDate
            };
        }
    }

    /// <summary>
    /// Handler for GetRestaurantByIdQuery
    /// Retrieves a specific restaurant by ID
    /// </summary>
    public class GetRestaurantByIdQueryHandler : IRequestHandler<GetRestaurantByIdQuery, RestaurantDto?>
    {
        private readonly IRestaurantRepository _repository;

        public GetRestaurantByIdQueryHandler(IRestaurantRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RestaurantDto?> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                throw new ArgumentException("Invalid restaurant ID. ID must be greater than 0.", nameof(request.Id));

            var restaurant = await _repository.GetByIdAsync(request.Id);
            
            if (restaurant == null)
                return null;

            return MapToDto(restaurant);
        }

        private static RestaurantDto MapToDto(Restaurant restaurant)
        {
            return new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                City = restaurant.City,
                IsActive = restaurant.IsActive,
                CreatedDate = restaurant.CreatedDate
            };
        }
    }

    /// <summary>
    /// Handler for GetRestaurantsWithReviewsQuery
    /// Retrieves all restaurants with their related reviews
    /// Uses EF Core Include() for eager loading
    /// </summary>
    public class GetRestaurantsWithReviewsQueryHandler : IRequestHandler<GetRestaurantsWithReviewsQuery, IEnumerable<RestaurantWithReviewsDto>>
    {
        private readonly IRestaurantRepository _repository;

        public GetRestaurantsWithReviewsQueryHandler(IRestaurantRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<RestaurantWithReviewsDto>> Handle(GetRestaurantsWithReviewsQuery request, CancellationToken cancellationToken)
        {
            var restaurants = await _repository.GetAllRestaurantsWithReviewsAsync();
            return restaurants.Select(MapToRestaurantWithReviewsDto).ToList();
        }

        private static RestaurantWithReviewsDto MapToRestaurantWithReviewsDto(Restaurant restaurant)
        {
            var reviewDtos = restaurant.Reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                RestaurantId = r.RestaurantId,
                ReviewerName = r.ReviewerName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedDate = r.CreatedDate
            }).ToList();

            var averageRating = reviewDtos.Any() ? reviewDtos.Average(r => r.Rating) : 0;

            return new RestaurantWithReviewsDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                City = restaurant.City,
                IsActive = restaurant.IsActive,
                CreatedDate = restaurant.CreatedDate,
                Reviews = reviewDtos,
                AverageRating = Math.Round(averageRating, 2),
                ReviewCount = reviewDtos.Count
            };
        }
    }
}
