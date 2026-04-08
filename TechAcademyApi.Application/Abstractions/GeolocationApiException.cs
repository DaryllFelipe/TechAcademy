namespace TechAcademyApi.Application.Abstractions
{
    /// <summary>
    /// Exception thrown when external geolocation API call fails
    /// Used by IGeolocationAdapter implementations
    /// </summary>
    public class GeolocationApiException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; }
        public string ResponseContent { get; }

        public GeolocationApiException(string message, System.Net.HttpStatusCode statusCode, string content)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = content;
        }
    }
}
