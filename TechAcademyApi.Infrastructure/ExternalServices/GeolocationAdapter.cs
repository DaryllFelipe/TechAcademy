using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TechAcademyApi.Application.Abstractions;

namespace TechAcademyApi.Infrastructure.ExternalServices
{
    /// <summary>
    /// Adapter implementation for external geolocation and weather APIs
    /// Fetches city data from public APIs without requiring authentication keys
    /// Uses open-meteo.com for weather and geolocation APIs
    /// Implements comprehensive error handling and logging
    /// </summary>
    public sealed class GeolocationAdapter : IGeolocationAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeolocationAdapter> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public GeolocationAdapter(HttpClient httpClient, ILogger<GeolocationAdapter> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fetches geolocation data using OpenMeteo Geocoding API
        /// </summary>
        public async Task<CityGeolocationData> GetGeolocationAsync(string city, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                _logger.LogError("Geolocation API called with null or empty city name.");
                throw new ArgumentException("City must be provided.", nameof(city));
            }

            _logger.LogDebug("Fetching geolocation data for city: {City}", city);

            try
            {
                // Use OpenMeteo Geocoding API (free, no key required)
                var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
                
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Geolocation API returned error for city '{City}': Status {StatusCode}. Response: {Response}",
                        city, response.StatusCode, content);
                    throw new GeolocationApiException("Geolocation API returned an error.", response.StatusCode, content);
                }

                var result = JsonSerializer.Deserialize<GeocodeResponse>(content, JsonOptions);
                
                if (result?.Results == null || result.Results.Count == 0)
                {
                    _logger.LogWarning("No geolocation data found for city: {City}", city);
                    throw new GeolocationApiException("No geolocation data found for city.", response.StatusCode, content);
                }

                var firstResult = result.Results[0];
                _logger.LogDebug("Successfully retrieved geolocation for city: {City} (Latitude: {Latitude}, Longitude: {Longitude})",
                    firstResult.Name ?? city, firstResult.Latitude, firstResult.Longitude);

                return new CityGeolocationData
                {
                    City = firstResult.Name ?? city,
                    Country = firstResult.Country ?? "Unknown",
                    Latitude = firstResult.Latitude,
                    Longitude = firstResult.Longitude,
                    Region = firstResult.Admin1 ?? "Unknown",
                    Timezone = firstResult.Timezone ?? "UTC"
                };
            }
            catch (GeolocationApiException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when fetching geolocation for city '{City}'. This may indicate network issues or the API is unreachable.", city);
                throw new GeolocationApiException($"Network error fetching geolocation for city '{city}'.", System.Net.HttpStatusCode.ServiceUnavailable, ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout when fetching geolocation for city '{City}'.", city);
                throw new GeolocationApiException($"Request timeout fetching geolocation for city '{city}'.", System.Net.HttpStatusCode.RequestTimeout, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching geolocation for city '{City}'.", city);
                throw new GeolocationApiException($"Error fetching geolocation for city '{city}'.", System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Fetches weather data using OpenMeteo Weather API
        /// </summary>
        public async Task<CityWeatherData> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                _logger.LogError("Weather API called with null or empty city name.");
                throw new ArgumentException("City must be provided.", nameof(city));
            }

            _logger.LogDebug("Fetching weather data for city: {City}", city);

            try
            {
                // First, get coordinates for the city
                var geoData = await GetGeolocationAsync(city, cancellationToken);

                // Then fetch weather for those coordinates
                var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={geoData.Latitude}&longitude={geoData.Longitude}&current=temperature_2m,relative_humidity_2m,weather_code,wind_speed_10m&temperature_unit=fahrenheit";

                _logger.LogDebug("Requesting weather data from OpenMeteo for coordinates: {Latitude}, {Longitude}", geoData.Latitude, geoData.Longitude);

                using var response = await _httpClient.GetAsync(weatherUrl, cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Weather API returned error for city '{City}': Status {StatusCode}. Response: {Response}",
                        city, response.StatusCode, content);
                    throw new GeolocationApiException("Weather API returned an error.", response.StatusCode, content);
                }

                var result = JsonSerializer.Deserialize<WeatherResponse>(content, JsonOptions);

                if (result?.Current == null)
                {
                    _logger.LogWarning("No weather data found in API response for city: {City}", city);
                    throw new GeolocationApiException("No weather data found in response.", response.StatusCode, content);
                }

                _logger.LogDebug("Successfully retrieved weather for city '{City}': Temp={Temperature}°F, Condition={Condition}",
                    city, result.Current.Temperature2m, GetWeatherCondition(result.Current.WeatherCode));

                return new CityWeatherData
                {
                    City = city,
                    Temperature = result.Current.Temperature2m,
                    Humidity = result.Current.RelativeHumidity2m,
                    WindSpeed = result.Current.WindSpeed10m,
                    Condition = GetWeatherCondition(result.Current.WeatherCode),
                    WeatherDescription = GetWeatherDescription(result.Current.WeatherCode)
                };
            }
            catch (GeolocationApiException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when fetching weather for city '{City}'.", city);
                throw new GeolocationApiException($"Network error fetching weather for city '{city}'.", System.Net.HttpStatusCode.ServiceUnavailable, ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout when fetching weather for city '{City}'.", city);
                throw new GeolocationApiException($"Request timeout fetching weather for city '{city}'.", System.Net.HttpStatusCode.RequestTimeout, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching weather for city '{City}'.", city);
                throw new GeolocationApiException($"Error fetching weather for city '{city}'.", System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Fetches combined geolocation and weather data for a city
        /// </summary>
        public async Task<CityExternalData> GetCityDataAsync(string city, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                _logger.LogError("GetCityDataAsync called with null or empty city name.");
                throw new ArgumentException("City must be provided.", nameof(city));
            }

            _logger.LogInformation("Starting to fetch combined geolocation and weather data for city: {City}", city);

            try
            {
                var geolocation = await GetGeolocationAsync(city, cancellationToken);
                var weather = await GetWeatherAsync(city, cancellationToken);

                _logger.LogInformation("Successfully fetched all external data for city '{City}'.", city);

                return new CityExternalData
                {
                    City = city,
                    Geolocation = geolocation,
                    Weather = weather
                };
            }
            catch (GeolocationApiException ex)
            {
                _logger.LogError(ex, "API error fetching city data for '{City}': {ErrorMessage}", city, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching city data for '{City}'.", city);
                throw new GeolocationApiException($"Error fetching city data for '{city}'.", System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Converts WMO weather code to human-readable condition
        /// </summary>
        private static string GetWeatherCondition(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "Clear sky",
                1 or 2 => "Partly cloudy",
                3 => "Overcast",
                45 or 48 => "Foggy",
                51 or 53 or 55 => "Light rain",
                61 or 63 => "Rain",
                65 => "Heavy rain",
                71 or 73 => "Light snow",
                75 => "Heavy snow",
                77 => "Snow grains",
                80 or 81 => "Showers",
                82 => "Heavy showers",
                85 or 86 => "Snow showers",
                95 or 96 or 99 => "Thunderstorm",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets detailed weather description from WMO code
        /// </summary>
        private static string GetWeatherDescription(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "Clear sky with excellent visibility",
                1 => "Mainly clear with some clouds",
                2 => "Partly cloudy conditions",
                3 => "Completely overcast",
                45 => "Foggy conditions",
                51 => "Light drizzle",
                61 => "Slight rain",
                80 => "Rain showers",
                95 => "Thunderstorm",
                _ => "Weather conditions variable"
            };
        }

        // Internal DTOs for mapping JSON responses
        private class GeocodeResponse
        {
            [JsonPropertyName("results")]
            public List<GeocodeResult> Results { get; set; } = new();
        }

        private class GeocodeResult
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }

            [JsonPropertyName("country")]
            public string? Country { get; set; }

            [JsonPropertyName("admin1")]
            public string? Admin1 { get; set; }

            [JsonPropertyName("timezone")]
            public string? Timezone { get; set; }
        }

        private class WeatherResponse
        {
            [JsonPropertyName("current")]
            public CurrentWeather? Current { get; set; }
        }

        private class CurrentWeather
        {
            [JsonPropertyName("temperature_2m")]
            public double Temperature2m { get; set; }

            [JsonPropertyName("relative_humidity_2m")]
            public int RelativeHumidity2m { get; set; }

            [JsonPropertyName("weather_code")]
            public int WeatherCode { get; set; }

            [JsonPropertyName("wind_speed_10m")]
            public double WindSpeed10m { get; set; }
        }
    }
}
