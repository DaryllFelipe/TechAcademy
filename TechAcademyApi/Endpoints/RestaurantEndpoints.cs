using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TechAcademyApi.Application.Abstractions;
using TechAcademyApi.Application.DTOs;
using TechAcademyApi.Application.Features.Restaurants.Commands;
using TechAcademyApi.Application.Features.Restaurants.Queries;

namespace TechAcademyApi.Endpoints
{
    /// <summary>
    /// Restaurant API Endpoints
    /// Maps REST endpoints for restaurant CRUD operations using CQRS + MediatR pattern
    /// Follows Clean Architecture with Domain-Driven Design principles
    /// </summary>
    public static class RestaurantEndpoints
    {
        /// <summary>
        /// Maps all restaurant endpoints to the WebApplication
        /// </summary>
        public static void MapRestaurantEndpoints(this WebApplication app)
        {
            // Specific routes first, then general routes
            
            // Summary endpoints (highest priority)
            app.MapGet("/summary/total-restaurants", GetTotalRestaurants)
                .WithName("GetTotalRestaurants");

            app.MapGet("/summary/reviews-per-restaurant", GetReviewsPerRestaurant)
                .WithName("GetReviewsPerRestaurant");

            app.MapGet("/summary/most-recent-review", GetMostRecentReview)
                .WithName("GetMostRecentReview");

            // Named query endpoints
            app.MapGet("/restaurants-with-reviews", GetRestaurantsWithReviews)
                .WithName("GetRestaurantsWithReviews");

            app.MapGet("/restaurants-with-external-data", GetRestaurantsWithExternalData)
                .WithName("GetRestaurantsWithExternalData");

            // Parameterized endpoints (before base endpoints)
            app.MapGet("/restaurants/{id}", GetRestaurantById)
                .WithName("GetRestaurantById");

            app.MapPut("/restaurants/{id}", UpdateRestaurant)
                .WithName("UpdateRestaurant")
                .Accepts<UpdateRestaurantRequest>("application/json")
                .Produces<RestaurantDto>()
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest);

            app.MapDelete("/restaurants/{id}", SoftDeleteRestaurant)
                .WithName("DeleteRestaurant")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            // Base endpoints (lowest priority)
            app.MapGet("/restaurants", GetAllRestaurants)
                .WithName("GetAllRestaurants");

            app.MapPost("/restaurants", CreateRestaurant)
                .WithName("CreateRestaurant")
                .Accepts<CreateRestaurantRequest>("application/json")
                .Produces<RestaurantDto>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// GET /restaurants - Retrieve all restaurants
        /// Dispatches GetAllRestaurantsQuery through MediatR
        /// </summary>
        private static async Task<IResult> GetAllRestaurants(IMediator mediator)
        {
            try
            {
                var query = new GetAllRestaurantsQuery();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /restaurants-with-reviews - Retrieve all restaurants with their reviews
        /// Dispatches GetRestaurantsWithReviewsQuery through MediatR
        /// Uses EF Core Include() for eager loading
        /// </summary>
        private static async Task<IResult> GetRestaurantsWithReviews(IMediator mediator)
        {
            try
            {
                var query = new GetRestaurantsWithReviewsQuery();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /restaurants-with-external-data - Retrieve all restaurants with external geolocation and weather data
        /// Dispatches GetRestaurantsWithExternalDataQuery through MediatR
        /// Fetches geolocation and weather data from OpenMeteo APIs for each restaurant's city
        /// Gracefully handles individual city API failures without failing entire operation
        /// Returns 503 Service Unavailable if external APIs are unreachable
        /// Returns 500 Internal Server Error for unexpected server errors
        /// </summary>
        private static async Task<IResult> GetRestaurantsWithExternalData(IMediator mediator, [FromServices] ILogger logger)
        {
            try
            {
                logger.LogInformation("GET /restaurants-with-external-data endpoint called.");
                var query = new GetRestaurantsWithExternalDataQuery();
                var result = await mediator.Send(query);
                
                logger.LogInformation("Successfully retrieved restaurants with external data.");
                return Results.Ok(result);
            }
            catch (GeolocationApiException ex)
            {
                logger.LogError(ex, "External geolocation API error: Status {StatusCode}", ex.StatusCode);
                
                // Map external API status codes to HTTP status codes
                int statusCode = ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
                    System.Net.HttpStatusCode.RequestTimeout => StatusCodes.Status504GatewayTimeout,
                    System.Net.HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };
                
                var errorResponse = new { 
                    error = "External API error",
                    message = ex.Message,
                    details = ex.ResponseContent
                };
                
                return Results.Json(errorResponse, statusCode: statusCode);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Invalid operation: {Error}", ex.Message);
                return Results.BadRequest(new { error = "Invalid operation", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid argument: {Error}", ex.Message);
                return Results.BadRequest(new { error = "Invalid argument", message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in GetRestaurantsWithExternalData endpoint.");
                
                var errorResponse = new { 
                    error = "Internal server error",
                    message = "An unexpected error occurred while fetching restaurants with external data."
                };
                
                return Results.Json(errorResponse, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// GET /restaurants/{id} - Retrieve a specific restaurant by ID
        /// Dispatches GetRestaurantByIdQuery through MediatR
        /// </summary>
        private static async Task<IResult> GetRestaurantById(int id, IMediator mediator)
        {
            try
            {
                var query = new GetRestaurantByIdQuery(id);
                var result = await mediator.Send(query);

                if (result == null)
                    return Results.NotFound(new { error = $"Restaurant with ID {id} not found." });

                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /restaurants - Create a new restaurant
        /// Dispatches CreateRestaurantCommand through MediatR
        /// </summary>
        private static async Task<IResult> CreateRestaurant(
            CreateRestaurantRequest request,
            IMediator mediator)
        {
            try
            {
                var command = new CreateRestaurantCommand(request.Name, request.City);
                var result = await mediator.Send(command);
                return Results.Created($"/restaurants/{result.Id}", result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT /restaurants/{id} - Update an existing restaurant
        /// Dispatches UpdateRestaurantCommand through MediatR
        /// </summary>
        private static async Task<IResult> UpdateRestaurant(
            int id,
            UpdateRestaurantRequest request,
            IMediator mediator)
        {
            try
            {
                var command = new UpdateRestaurantCommand(id, request.Name, request.City, request.IsActive);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /restaurants/{id} - Soft delete a restaurant
        /// Dispatches DeleteRestaurantCommand through MediatR
        /// Sets IsActive = false instead of physically deleting
        /// </summary>
        private static async Task<IResult> SoftDeleteRestaurant(int id, IMediator mediator)
        {
            try
            {
                var command = new DeleteRestaurantCommand(id);
                await mediator.Send(command);
                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /summary/total-restaurants - Retrieve total count of restaurants
        /// Dispatches GetTotalRestaurantsQuery through MediatR
        /// </summary>
        private static async Task<IResult> GetTotalRestaurants(IMediator mediator, [FromServices] ILogger logger)
        {
            try
            {
                logger.LogInformation("GET /summary/total-restaurants endpoint called.");
                var query = new GetTotalRestaurantsQuery();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving total restaurants count.");
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /summary/reviews-per-restaurant - Retrieve review count per restaurant
        /// Dispatches GetReviewsPerRestaurantQuery through MediatR
        /// Returns list of restaurants with their review counts, sorted by name
        /// </summary>
        private static async Task<IResult> GetReviewsPerRestaurant(IMediator mediator, [FromServices] ILogger logger)
        {
            try
            {
                logger.LogInformation("GET /summary/reviews-per-restaurant endpoint called.");
                var query = new GetReviewsPerRestaurantQuery();
                var result = await mediator.Send(query);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving reviews per restaurant.");
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /summary/most-recent-review - Retrieve the most recent review
        /// Dispatches GetMostRecentReviewQuery through MediatR
        /// Returns the latest review with restaurant context
        /// Returns null if no reviews exist
        /// </summary>
        private static async Task<IResult> GetMostRecentReview(IMediator mediator, [FromServices] ILogger logger)
        {
            try
            {
                logger.LogInformation("GET /summary/most-recent-review endpoint called.");
                var query = new GetMostRecentReviewQuery();
                var result = await mediator.Send(query);
                
                if (result == null)
                    return Results.NotFound(new { message = "No reviews found in the system." });
                
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving most recent review.");
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}