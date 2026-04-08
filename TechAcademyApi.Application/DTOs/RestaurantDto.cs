namespace TechAcademyApi.Application.DTOs
{
    /// <summary>
    /// Restaurant Data Transfer Object
    /// Used for API request/response payloads to decouple entity model from API contracts
    /// </summary>
    public class RestaurantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Create Restaurant Request DTO
    /// Used for POST requests to create new restaurants
    /// </summary>
    public class CreateRestaurantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    /// <summary>
    /// Update Restaurant Request DTO
    /// Used for PUT requests to update existing restaurants
    /// </summary>
    public class UpdateRestaurantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Review Data Transfer Object
    /// Used in responses that include review information
    /// </summary>
    public class ReviewDto
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Restaurant with Reviews DTO
    /// Used for responses that include both restaurant details and its reviews
    /// </summary>
    public class RestaurantWithReviewsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();

        /// <summary>
        /// Average rating calculated from all reviews
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of reviews
        /// </summary>
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// External location data DTO
    /// Contains geolocation and weather information for a city
    /// </summary>
    public class CityExternalDataDto
    {
        public string City { get; set; } = string.Empty;
        public GeolocationDto? Geolocation { get; set; }
        public WeatherDto? Weather { get; set; }
    }

    /// <summary>
    /// Geolocation DTO
    /// Contains latitude, longitude, country, and timezone info
    /// </summary>
    public class GeolocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Weather DTO
    /// Contains temperature, condition, humidity, and wind information
    /// </summary>
    public class WeatherDto
    {
        public double Temperature { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
    }

    /// <summary>
    /// Restaurant with External Data DTO
    /// Combines restaurant information with geolocation and weather data
    /// </summary>
    public class RestaurantWithExternalDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// External location and weather data for the restaurant's city
        /// </summary>
        public CityExternalDataDto? ExternalData { get; set; }
    }

    /// <summary>
    /// Summary DTO - Total restaurants count
    /// </summary>
    public class TotalRestaurantsSummaryDto
    {
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Summary DTO - Reviews per restaurant
    /// Used in list responses showing restaurant name and review count
    /// </summary>
    public class RestaurantReviewCountDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// Summary DTO - Most recent review
    /// Contains latest review with restaurant name context
    /// </summary>
    public class MostRecentReviewDto
    {
        public int ReviewId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
