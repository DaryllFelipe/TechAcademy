using MediatR;
using Microsoft.Extensions.Logging;
using TechAcademyApi.Application.Abstractions;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Queries;
using TechAcademyApi.Core.Abstractions;
using TechAcademyApi.Core.Entities;

namespace TechAcademyApi.Application.Features.Restaurants.Handlers
{
    /// <summary>
    /// Handler for GetRestaurantsWithExternalDataQuery
    /// Fetches all restaurants and combines them with external geolocation/weather data
    /// Calls the GeolocationAdapter to fetch city data from public APIs
    /// Gracefully handles individual city API failures without failing entire operation
    /// </summary>
    public class GetRestaurantsWithExternalDataQueryHandler : IRequestHandler<GetRestaurantsWithExternalDataQuery, IEnumerable<RestaurantWithExternalDataDto>>
    {
        private readonly IRestaurantRepository _repository;
        private readonly IGeolocationAdapter _geolocationAdapter;
        private readonly ILogger<GetRestaurantsWithExternalDataQueryHandler> _logger;

        public GetRestaurantsWithExternalDataQueryHandler(
            IRestaurantRepository repository,
            IGeolocationAdapter geolocationAdapter,
            ILogger<GetRestaurantsWithExternalDataQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _geolocationAdapter = geolocationAdapter ?? throw new ArgumentNullException(nameof(geolocationAdapter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RestaurantWithExternalDataDto>> Handle(
            GetRestaurantsWithExternalDataQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting to fetch restaurants with external geolocation and weather data.");
            
            try
            {
                // Fetch all restaurants
                var restaurants = await _repository.GetAllAsync();

                if (restaurants == null || !restaurants.Any())
                {
                    _logger.LogInformation("No restaurants found in the database.");
                    return Enumerable.Empty<RestaurantWithExternalDataDto>();
                }

                _logger.LogInformation("Retrieved {RestaurantCount} restaurants. Fetching external data for each city.", restaurants.Count());

                // For each restaurant, fetch external data and combine
                var restaurantsWithData = new List<RestaurantWithExternalDataDto>();
                var failedCities = new List<(string City, string Error)>();

                foreach (var restaurant in restaurants)
                {
                    var dto = MapToDto(restaurant);

                    try
                    {
                        // Fetch external data for the city
                        var externalData = await _geolocationAdapter.GetCityDataAsync(restaurant.City, cancellationToken);
                        
                        // Map external data to DTO
                        dto.ExternalData = MapExternalDataToDto(externalData);
                        _logger.LogDebug("Successfully fetched external data for city '{City}'.", restaurant.City);
                    }
                    catch (GeolocationApiException ex)
                    {
                        // Log error and continue - don't fail entire operation if one city fails
                        _logger.LogWarning(ex, "Failed to fetch external API data for city '{City}'. Status: {StatusCode}. Response: {Response}",
                            restaurant.City, ex.StatusCode, ex.ResponseContent);
                        failedCities.Add((restaurant.City, ex.Message));
                        dto.ExternalData = null;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning(ex, "Invalid input when fetching external data for city '{City}'.", restaurant.City);
                        failedCities.Add((restaurant.City, ex.Message));
                        dto.ExternalData = null;
                    }
                    catch (Exception ex)
                    {
                        // Log unexpected errors and continue
                        _logger.LogError(ex, "Unexpected error fetching external data for city '{City}'.", restaurant.City);
                        failedCities.Add((restaurant.City, ex.Message));
                        dto.ExternalData = null;
                    }

                    restaurantsWithData.Add(dto);
                }

                if (failedCities.Any())
                {
                    _logger.LogWarning("Failed to fetch external data for {FailedCityCount} out of {TotalCityCount} cities. Failed cities: {FailedCities}",
                        failedCities.Count, restaurants.Count(), string.Join(", ", failedCities.Select(f => f.City)));
                }
                else
                {
                    _logger.LogInformation("Successfully fetched external data for all {CityCount} restaurants.", restaurantsWithData.Count);
                }

                return restaurantsWithData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in GetRestaurantsWithExternalDataQueryHandler.");
                throw;
            }
        }

        private static RestaurantWithExternalDataDto MapToDto(Restaurant restaurant)
        {
            return new RestaurantWithExternalDataDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                City = restaurant.City,
                IsActive = restaurant.IsActive,
                CreatedDate = restaurant.CreatedDate,
                ExternalData = null
            };
        }

        private static CityExternalDataDto MapExternalDataToDto(CityExternalData externalData)
        {
            return new CityExternalDataDto
            {
                City = externalData.City,
                Geolocation = externalData.Geolocation != null ? new GeolocationDto
                {
                    Latitude = externalData.Geolocation.Latitude,
                    Longitude = externalData.Geolocation.Longitude,
                    Country = externalData.Geolocation.Country,
                    Region = externalData.Geolocation.Region,
                    Timezone = externalData.Geolocation.Timezone
                } : null,
                Weather = externalData.Weather != null ? new WeatherDto
                {
                    Temperature = externalData.Weather.Temperature,
                    Condition = externalData.Weather.Condition,
                    Description = externalData.Weather.WeatherDescription,
                    Humidity = externalData.Weather.Humidity,
                    WindSpeed = externalData.Weather.WindSpeed
                } : null
            };
        }
    }
}
