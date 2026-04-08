using MediatR;
using TechAcademyApi.Application.DTOs;

namespace TechAcademyApi.Application.Features.Restaurants.Commands
{
    /// <summary>
    /// Command to create a new restaurant
    /// Part of CQRS pattern - represents a write operation
    /// </summary>
    public class CreateRestaurantCommand : IRequest<RestaurantDto>
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public CreateRestaurantCommand(string name, string city)
        {
            Name = name;
            City = city;
        }
    }

    /// <summary>
    /// Command to update an existing restaurant
    /// </summary>
    public class UpdateRestaurantCommand : IRequest<RestaurantDto>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public UpdateRestaurantCommand(int id, string name, string city, bool isActive)
        {
            Id = id;
            Name = name;
            City = city;
            IsActive = isActive;
        }
    }

    /// <summary>
    /// Command to soft delete a restaurant (mark as inactive)
    /// </summary>
    public class DeleteRestaurantCommand : IRequest<Unit>
    {
        public int Id { get; set; }

        public DeleteRestaurantCommand(int id)
        {
            Id = id;
        }
    }
}
