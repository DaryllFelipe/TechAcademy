namespace TechAcademyApi.Application.Abstractions
{
    /// <summary>
    /// Adapter interface for external geolocation and weather API calls
    /// Defines contract for fetching city-based external data
    /// Follows Adapter pattern for decoupling from external API specifics
    /// </summary>
    public interface IGeolocationAdapter
    {
        /// <summary>
        /// Fetches geolocation data for a city
        /// Uses IP-based or city-name-based geolocation API
        /// </summary>
        Task<CityGeolocationData> GetGeolocationAsync(string city, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches weather data for a city
        /// Uses OpenWeather API or similar service
        /// </summary>
        Task<CityWeatherData> GetWeatherAsync(string city, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches combined geolocation and weather data for a city
        /// Aggregates results from multiple external APIs
        /// </summary>
        Task<CityExternalData> GetCityDataAsync(string city, CancellationToken cancellationToken = default);
    }
}
