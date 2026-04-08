namespace TechAcademyApi.Application.Abstractions
{
    /// <summary>
    /// Geolocation data returned from external API for a city
    /// </summary>
    public class CityGeolocationData
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Region { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Weather information for a city from OpenWeather API
    /// </summary>
    public class CityWeatherData
    {
        public string City { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public string WeatherDescription { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string Condition { get; set; } = string.Empty;
    }

    /// <summary>
    /// Combined external API data for a city
    /// Includes both geolocation and weather information
    /// </summary>
    public class CityExternalData
    {
        public string City { get; set; } = string.Empty;
        public CityGeolocationData? Geolocation { get; set; }
        public CityWeatherData? Weather { get; set; }
    }
}
