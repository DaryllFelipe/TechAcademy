using MediatR;
using TechAcademyApi.Application.DTOs;

namespace TechAcademyApi.Application.Features.Restaurants.Queries
{
    /// <summary>
    /// CQRS Query to retrieve the most recent review
    /// Returns the latest review added to the system with restaurant context
    /// Returns null if no reviews exist
    /// </summary>
    public record GetMostRecentReviewQuery : IRequest<MostRecentReviewDto?>;
}
