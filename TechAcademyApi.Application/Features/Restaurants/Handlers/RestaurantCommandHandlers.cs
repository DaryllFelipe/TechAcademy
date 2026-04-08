using MediatR;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Commands;
using TechAcademyApi.Core.Abstractions;
using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Application.Features.Restaurants.Handlers
{
    /// <summary>
    /// Handler for CreateRestaurantCommand
    /// Implements business logic for creating a new restaurant
    /// MediatR will automatically route CreateRestaurantCommand to this handler
    /// </summary>
    public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand, RestaurantDto>
    {
        private readonly IRestaurantRepository _repository;

        public CreateRestaurantCommandHandler(IRestaurantRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RestaurantDto> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Restaurant name is required and cannot be empty.", nameof(request.Name));

            if (string.IsNullOrWhiteSpace(request.City))
                throw new ArgumentException("City is required and cannot be empty.", nameof(request.City));

            if (request.Name.Length > 150)
                throw new ArgumentException("Restaurant name cannot exceed 150 characters.", nameof(request.Name));

            if (request.City.Length > 100)
                throw new ArgumentException("City cannot exceed 100 characters.", nameof(request.City));

            // Create entity
            var restaurant = new Restaurant
            {
                Name = request.Name.Trim(),
                City = request.City.Trim(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            // Save to database
            var createdRestaurant = await _repository.AddAsync(restaurant);
            await _repository.SaveChangesAsync();

            // Return DTO
            return MapToDto(createdRestaurant);
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
    /// Handler for UpdateRestaurantCommand
    /// Implements business logic for updating an existing restaurant
    /// </summary>
    public class UpdateRestaurantCommandHandler : IRequestHandler<UpdateRestaurantCommand, RestaurantDto>
    {
        private readonly IRestaurantRepository _repository;

        public UpdateRestaurantCommandHandler(IRestaurantRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RestaurantDto> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
        {
            // Validation
            if (request.Id <= 0)
                throw new ArgumentException("Invalid restaurant ID. ID must be greater than 0.", nameof(request.Id));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Restaurant name is required and cannot be empty.", nameof(request.Name));

            if (string.IsNullOrWhiteSpace(request.City))
                throw new ArgumentException("City is required and cannot be empty.", nameof(request.City));

            if (request.Name.Length > 150)
                throw new ArgumentException("Restaurant name cannot exceed 150 characters.", nameof(request.Name));

            if (request.City.Length > 100)
                throw new ArgumentException("City cannot exceed 100 characters.", nameof(request.City));

            // Fetch existing restaurant
            var restaurant = await _repository.GetByIdAsync(request.Id);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {request.Id} not found.");

            // Update properties
            restaurant.Name = request.Name.Trim();
            restaurant.City = request.City.Trim();
            restaurant.IsActive = request.IsActive;

            // Save changes
            await _repository.UpdateAsync(restaurant);
            await _repository.SaveChangesAsync();

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
    /// Handler for DeleteRestaurantCommand
    /// Implements soft delete logic (marks restaurant as inactive)
    /// </summary>
    public class DeleteRestaurantCommandHandler : IRequestHandler<DeleteRestaurantCommand, Unit>
    {
        private readonly IRestaurantRepository _repository;

        public DeleteRestaurantCommandHandler(IRestaurantRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Unit> Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
        {
            // Validation
            if (request.Id <= 0)
                throw new ArgumentException("Invalid restaurant ID. ID must be greater than 0.", nameof(request.Id));

            // Fetch restaurant
            var restaurant = await _repository.GetByIdAsync(request.Id);
            if (restaurant == null)
                throw new KeyNotFoundException($"Restaurant with ID {request.Id} not found.");

            // Soft delete: mark as inactive
            restaurant.IsActive = false;

            // Save changes
            await _repository.UpdateAsync(restaurant);
            await _repository.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
