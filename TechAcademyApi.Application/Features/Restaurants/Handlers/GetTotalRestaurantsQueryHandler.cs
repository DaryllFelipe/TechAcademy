using MediatR;
using Microsoft.Extensions.Logging;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Queries;
using TechAcademyApi.Core.Abstractions;

namespace TechAcademyApi.Application.Features.Restaurants.Handlers
{
    /// <summary>
    /// Handler for GetTotalRestaurantsQuery
    /// Retrieves total count of active restaurants from the database
    /// Uses LINQ to count restaurants
    /// </summary>
    public class GetTotalRestaurantsQueryHandler : IRequestHandler<GetTotalRestaurantsQuery, TotalRestaurantsSummaryDto>
    {
        private readonly IRestaurantRepository _repository;
        private readonly ILogger<GetTotalRestaurantsQueryHandler> _logger;

        public GetTotalRestaurantsQueryHandler(
            IRestaurantRepository repository,
            ILogger<GetTotalRestaurantsQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TotalRestaurantsSummaryDto> Handle(
            GetTotalRestaurantsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching total restaurant count.");
            
            try
            {
                var restaurants = await _repository.GetAllAsync();
                var totalCount = restaurants.Count(r => r.IsActive);

                _logger.LogInformation("Total restaurants count: {Count}", totalCount);

                return new TotalRestaurantsSummaryDto
                {
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching total restaurants count.");
                throw;
            }
        }
    }
}
