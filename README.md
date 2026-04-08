# TechAcademy API - Complete Solution Analysis

## 🎯 Executive Summary

**TechAcademy API** is a production-grade ASP.NET Core 8 Web API that implements **clean architecture**, **CQRS pattern**, and **DDD (Domain-Driven Design)** principles. It manages a restaurant system with reviews, ratings, and external geolocation/weather data enrichment using free public APIs.

---

## 📊 Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                  API LAYER (Presentation)                    │
│        RestaurantEndpoints.cs - HTTP Endpoints               │
│        Maps REST routes using Minimal APIs                   │
└────────────────────┬────────────────────────────────────────┘
                     │
    ┌────────────────▼─────────────────┐
    │    MediatR - CQRS Dispatcher     │
    │  (Routes queries/commands)       │
    └────────────────┬─────────────────┘
         ┌───────────┴───────────┐
         │                       │
┌────────▼──────────────┐  ┌────▼──────────────────┐
│  APPLICATION LAYER    │  │  INFRASTRUCTURE LAYER │
├───────────────────────┤  ├───────────────────────┤
│ Features/Restaurants/ │  │ Persistence/          │
│  ├─ Queries/          │  │  ├─ DbContext         │
│  │  ├─ GetAll         │  │  ├─ Repositories      │
│  │  ├─ GetById        │  │  └─ Migrations        │
│  │  ├─ GetWithReviews │  │                       │
│  │  └─ GetWithExtData │  │ ExternalServices/    │
│  │                    │  │  └─ GeolocationAdapter
│  ├─ Commands/         │  │                       │
│  │  ├─ Create         │  │ HttpClient Setup      │
│  │  ├─ Update         │  │ (Open-Meteo APIs)     │
│  │  └─ Delete         │  │                       │
│  │                    │  │                       │
│  ├─ Handlers/         │  │                       │
│  │  ├─ QueryHandlers  │  │                       │
│  │  └─ CmdHandlers    │  │                       │
│  │                    │  │                       │
│  ├─ DTOs/             │  │                       │
│  └─ Abstractions/     │  │                       │
└────────┬─────────────┘  └────┬───────────────────┘
         │                     │
         └────────┬────────────┘
                  │
        ┌─────────▼──────────┐
        │   CORE LAYER       │
        │  (Domain Models)   │
        ├────────────────────┤
        │ Entities:          │
        │  ├─ Restaurant     │
        │  └─ Review         │
        │                    │
        │ Abstractions:      │
        │  ├─ IRepository<T> │
        │  ├─ IRestaurant... │
        │  ├─ IReview...     │
        │  └─ IGeolocation...│
        └────────────────────┘
                  │
        ┌─────────▼──────────┐
        │    SQLite DB       │
        │  techacademy.db    │
        └────────────────────┘
```

### Layer Responsibilities

1. **Core Layer** - Innermost, stable domain models
2. **Application Layer** - Use cases, CQRS handlers, business logic
3. **Infrastructure Layer** - Data access, external APIs, persistence
4. **API Layer** - HTTP endpoints, request/response mapping

---

## 🔄 How the Solution Works

### 1. Application Startup Flow

```csharp
Program.cs:
1. CreateBuilder()
2. Load connection string from appsettings.json
3. Call AddTechAcademyServices(connectionString)
   ├─ AddApplicationLayer() - MediatR handler registration
   └─ AddInfrastructureLayer() - DbContext, repositories, HttpClient
4. Configure CORS - AllowAll policy
5. Setup logging - Console, minimum level: Information
6. Auto-run database migrations
7. Map all restaurant endpoints
8. app.Run() - Start server on http://localhost:5000 or https://localhost:5001
```

### 2. Request Processing Pipeline

#### Example: GET /restaurants (Retrieve all restaurants)

```
1. HTTP GET /restaurants arrives
   │
2. RestaurantEndpoints.MapRestaurantEndpoints() routes to GetAllRestaurants handler
   │
3. Handler extracts IMediator from DI container
   │
4. Creates Query: GetAllRestaurantsQuery()
   │
5. mediator.Send(query)
   │
6. MediatR looks up handler for GetAllRestaurantsQuery type
   │
7. Creates GetAllRestaurantsQueryHandler instance
   │
8. Calls handler.Handle(GetAllRestaurantsQuery, cancellationToken)
   │
9. Handler executes business logic:
   ├─ Calls IRestaurantRepository.GetAllAsync()
   ├─ Repository queries SQLite database via Entity Framework Core
   ├─ Maps Restaurant entities to RestaurantDto DTOs
   └─ Returns IEnumerable<RestaurantDto>
   │
10. Response converted to JSON by ASP.NET Core
    │
11. HTTP 200 OK with JSON body containing restaurants
```

#### Database Query Execution

```csharp
// Handler calls:
var restaurants = await _repository.GetAllAsync();

// Behind the scenes in GenericRepository<T>:
public async Task<IEnumerable<T>> GetAllAsync()
{
    return await _dbContext.Set<T>().ToListAsync();
}

// EF Core generates and executes SQL:
SELECT [r].[Id], [r].[Name], [r].[City], [r].[IsActive], [r].[CreatedDate]
FROM [Restaurants] AS [r]

// Results mapped to Restaurant entities, then to DTOs
```

---

## 📝 CQRS Pattern Implementation

### Queries (Read Operations)

```
Query Definition         Handler                 Database
─────────────────────────────────────────────────────────────

GetAllRestaurantsQuery
    │
    └─> GetAllRestaurantsQueryHandler
            │
            └─> _repository.GetAllAsync()
                    │
                    └─> SELECT * FROM Restaurants

GetRestaurantByIdQuery
    │
    └─> GetRestaurantByIdQueryHandler
            │
            └─> _repository.GetByIdAsync(id)
                    │
                    └─> SELECT * FROM Restaurants WHERE Id = ?

GetRestaurantsWithReviewsQuery
    │
    └─> GetRestaurantsWithReviewsQueryHandler
            │
            └─> _repository.GetAllRestaurantsWithReviewsAsync()
                    │
                    └─> SELECT * FROM Restaurants
                        INNER JOIN Reviews ON Restaurant.Id = Reviews.RestaurantId
                        (Eager loading with Include())

GetTotalRestaurantsQuery
    │
    └─> GetTotalRestaurantsQueryHandler
            │
            └─> Returns COUNT of restaurants

GetReviewsPerRestaurantQuery
    │
    └─> GetReviewsPerRestaurantQueryHandler
            │
            └─> Returns restaurant with review count and average rating
```

### Commands (Write Operations)

```
Command Definition           Handler                Database Action
─────────────────────────────────────────────────────────────────────

CreateRestaurantCommand(name, city)
    │
    └─> CreateRestaurantCommandHandler
            │
            ├─ Validate input
            ├─ Create new Restaurant entity
            ├─ _repository.AddAsync(restaurant)
            ├─ _repository.SaveChangesAsync()
            │  └─> INSERT INTO Restaurants...
            └─> Return RestaurantDto

UpdateRestaurantCommand(id, name, city, isActive)
    │
    └─> UpdateRestaurantCommandHandler
            │
            ├─ Validate input
            ├─ Fetch existing restaurant
            ├─ Update properties
            ├─ _repository.UpdateAsync(restaurant)
            ├─ _repository.SaveChangesAsync()
            │  └─> UPDATE Restaurants WHERE Id = ?
            └─> Return RestaurantDto

DeleteRestaurantCommand(id)
    │
    └─> DeleteRestaurantCommandHandler
            │
            ├─ Fetch restaurant
            ├─ Set IsActive = false (soft delete)
            ├─ _repository.UpdateAsync(restaurant)
            ├─ _repository.SaveChangesAsync()
            │  └─> UPDATE Restaurants SET IsActive = 0 WHERE Id = ?
            └─> Return success
```

### Benefits of CQRS

- **Clear intent** - Queries for reading, Commands for writing
- **Scalability** - Can optimize reads separately from writes
- **Testability** - Each handler can be tested independently
- **Maintainability** - Single responsibility per handler

---

## 🌍 External API Integration

### GeolocationAdapter Pattern

The adapter pattern abstracts external API calls, making them testable and replaceable:

```csharp
IGeolocationAdapter interface (defined in Application layer abstractions)
    │
    └─> GeolocationAdapter implementation (Infrastructure layer)
            │
            ├─> GetGeolocationAsync(city)
            │   └─> Calls: https://geocoding-api.open-meteo.com/v1/search
            │       Parses JSON response
            │       Returns: CityGeolocationData {City, Country, Latitude, Longitude, Timezone}
            │
            ├─> GetWeatherAsync(city)
            │   └─> Calls: https://api.open-meteo.com/v1/forecast
            │       Parses JSON response
            │       Returns: CityWeatherData {Temperature, Humidity, WindSpeed}
            │
            └─> GetCityDataAsync(city) [optional]
                └─> Combines geolocation + weather into one response
```

### Example Flow: GET /restaurants-with-external-data

```
1. Endpoint receives request
   │
2. Dispatches GetRestaurantsWithExternalDataQuery
   │
3. GetRestaurantsWithExternalDataQueryHandler executes:
   ├─ Fetches all restaurants
   │
   └─ For each restaurant:
      ├─ Calls IGeolocationAdapter.GetGeolocationAsync(city)
      │  └─ HTTP request to OpenMeteo Geocoding API
      │     └─ Response: {latitude, longitude, country, timezone}
      │
      ├─ Calls IGeolocationAdapter.GetWeatherAsync(city)
      │  └─ HTTP request to OpenMeteo Weather API (using coordinates)
      │     └─ Response: {temperature, humidity, wind_speed}
      │
      └─ Combines data into RestaurantWithExternalDataDto
   │
4. Returns complete data with enrichment
```

### Error Handling in External APIs

```
GeolocationApiException wraps:
├─ HTTP status code
├─ Response content
└─ Custom error message

Endpoint maps to HTTP status:
├─ 503 Service Unavailable → External API down
├─ 504 Gateway Timeout → External API timeout
├─ 400 Bad Request → Invalid input
└─ 500 Internal Server Error → Unexpected error
```

---

## 💾 Database Design

### Entity Relationships

```
┌──────────────────────────┐
│      Restaurant          │
├──────────────────────────┤
│ Id (PK) - int            │
│ Name - string(150)       │
│ City - string(100)       │
│ IsActive - bool          │
│ CreatedDate - DateTime   │
│ Reviews (Navigation)     │──┐
└──────────────────────────┘  │ 1 to Many
                              │
                 ┌────────────┘
                 │
        ┌────────▼──────────┐
        │     Review        │
        ├───────────────────┤
        │ Id (PK) - int     │
        │ RestaurantId (FK) │
        │ ReviewerName      │
        │ Rating (1-5)      │
        │ Comment           │
        │ CreatedDate       │
        └───────────────────┘
```

### Database Features

- **SQLite** - File-based, zero-config database
- **Auto-migrations** - Run on startup via MigrateAsync()
- **Soft delete** - IsActive flag, not physical deletion
- **Timestamps** - CreatedDate automatically set
- **Constraints** - String length, required fields, rating range (1-5)

---

## 🔐 Dependency Injection

### Service Registration Flow

```
Program.cs
└─> AddTechAcademyServices(connectionString)
    │
    ├─> AddApplicationLayer() (in TechAcademyApi.Application)
    │   └─> services.AddMediatR(typeof(GetAllRestaurantsQuery).Assembly)
    │       └─> Auto-discovers and registers all handlers
    │
    └─> AddInfrastructureLayer(connectionString) (in TechAcademyApi.Infrastructure)
        │
        ├─> services.AddDbContext<TechAcademyDbContext>(...)
        │   └─> SQLite configuration
        │
        ├─> services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>))
        │   └─> Generic repository for any entity type
        │
        ├─> services.AddScoped<IRestaurantRepository, RestaurantRepository>()
        │   └─ Specialized repository for complex restaurant queries
        │
        ├─> services.AddScoped<IReviewRepository, ReviewRepository>()
        │   └─ Specialized repository for review operations
        │
        └─> services.AddHttpClient<IGeolocationAdapter, GeolocationAdapter>()
            └─> HttpClient for external API calls
```

### Lifetime Scopes

- **Transient** - New instance every time (not used)
- **Scoped** - One per HTTP request (Repositories, DbContext)
- **Singleton** - Single instance for app lifetime (Configuration, ILogger factory)

---

## 🛣️ API Endpoints Reference

### Restaurant Management

| Method | Endpoint | CQRS | Purpose | Status |
|--------|----------|------|---------|--------|
| GET | `/restaurants` | Query | Get all restaurants | 200 OK |
| GET | `/restaurants/{id}` | Query | Get specific restaurant | 200 OK / 404 |
| POST | `/restaurants` | Command | Create new | 201 Created / 400 |
| PUT | `/restaurants/{id}` | Command | Update | 200 OK / 404 / 400 |
| DELETE | `/restaurants/{id}` | Command | Soft delete | 204 No Content / 404 |

### Related Data Queries

| Method | Endpoint | CQRS | Purpose |
|--------|----------|------|---------|
| GET | `/restaurants-with-reviews` | Query | Get restaurants + reviews (eager loaded) |
| GET | `/restaurants-with-external-data` | Query | Get restaurants + geolocation + weather |

### Analytics & Summary

| Method | Endpoint | CQRS | Purpose |
|--------|----------|------|---------|
| GET | `/summary/total-restaurants` | Query | Count of all restaurants |
| GET | `/summary/reviews-per-restaurant` | Query | Review count per restaurant + avg rating |
| GET | `/summary/most-recent-review` | Query | Latest review across all restaurants |

### Health & Diagnostics

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/health` | Application health check |

---

## 🔑 Key Design Patterns Used

### 1. Clean Architecture
- **Separation of concerns** across layers
- **Dependencies flow inward** (API → App → Core)
- **Core layer independent** of external frameworks
- **Easy to test** - mock implementations

### 2. CQRS (Command Query Responsibility Segregation)
- **Queries** - Read operations, return data
- **Commands** - Write operations, modify state
- **Clear intent** - Understand what the code does
- **Scalable** - Can optimize reads/writes independently

### 3. Repository Pattern
```csharp
Generic:   IRepository<T>
           ├─ GetAllAsync()
           ├─ GetByIdAsync(id)
           ├─ AddAsync(entity)
           ├─ UpdateAsync(entity)
           ├─ DeleteAsync(entity)
           └─ SaveChangesAsync()

Specialized: IRestaurantRepository : IRepository<Restaurant>
             ├─ GetAllRestaurantsWithReviewsAsync()
             ├─ GetRestaurantWithReviewsAsync(id)
             └─ GetActiveRestaurantsAsync()
```

### 4. Adapter Pattern
```csharp
// Define what we want (abstraction)
IGeolocationAdapter
├─ GetGeolocationAsync(city)
├─ GetWeatherAsync(city)
└─ GetCityDataAsync(city)

// Implement external integration (adapter)
GeolocationAdapter : IGeolocationAdapter
└─ Uses HttpClient to call Open-Meteo APIs

// Benefit: Can swap implementation without changing business logic
```

### 5. Dependency Injection
- **Loose coupling** - Inject dependencies instead of creating
- **Testable** - Mock dependencies in unit tests
- **Flexible** - Change implementations without code changes
- **Managed lifecycle** - .NET handles disposal

### 6. DTO (Data Transfer Object) Pattern
```csharp
// Domain Entity (internal)
Restaurant entity {Id, Name, City, IsActive, CreatedDate, Reviews}

// Transfer Object (API contract)
RestaurantDto {Id, Name, City, IsActive, CreatedDate}
RestaurantWithReviewsDto {Id, Name, City, ReviewCount, AverageRating, Reviews}

// Benefits: Decouple API from internal representation
```

---

## 📊 Data Flow Example: Create Restaurant

```
Client Request
│
└─> POST /restaurants
    Content-Type: application/json
    {
      "name": "The Golden Fork",
      "city": "New York"
    }
    │
    └─> RestaurantEndpoints.CreateRestaurant(request)
        │
        └─> mediator.Send(new CreateRestaurantCommand(...))
            │
            └─> CreateRestaurantCommandHandler.Handle()
                │
                ├─ Validate: name not empty (✓)
                ├─ Validate: name <= 150 chars (✓)
                ├─ Validate: city not empty (✓)
                ├─ Validate: city <= 100 chars (✓)
                │
                └─> Create entity:
                    Restaurant {
                        Name = "The Golden Fork",
                        City = "New York",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                    │
                    └─> _repository.AddAsync(restaurant)
                        │
                        └─> _dbContext.Restaurants.Add(restaurant)
                        └─> _repository.SaveChangesAsync()
                            │
                            └─> await _dbContext.SaveChangesAsync()
                                │
                                └─> DATABASE: INSERT into Restaurants
                                    INSERT INTO Restaurants (Name, City, IsActive, CreatedDate)
                                    VALUES ('The Golden Fork', 'New York', 1, '2024-01-15T10:30:00Z')
                                    │
                                    └─> Returns Restaurant with Id = 1 (auto-generated)
                                        │
                                        └─> Map to DTO:
                                            RestaurantDto {
                                                Id = 1,
                                                Name = "The Golden Fork",
                                                City = "New York",
                                                IsActive = true,
                                                CreatedDate = DateTime.UtcNow
                                            }
                                            │
                                            └─> HTTP 201 Created
                                                Location: /restaurants/1
                                                Body: {...RestaurantDto...}
```

---

## 🔧 Configuration & Settings

### appsettings.json (Production)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=techacademy.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### appsettings.Development.json (Development)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### launchSettings.json (Local Debug)
```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## ✅ Error Handling Strategy

### Input Validation
```csharp
// In Commands/Queries:
if (string.IsNullOrWhiteSpace(request.Name))
    throw new ArgumentException("Restaurant name required");

if (request.Name.Length > 150)
    throw new ArgumentException("Name cannot exceed 150 chars");
```

### Business Logic Errors
```csharp
// In Handlers:
var restaurant = await _repository.GetByIdAsync(request.Id);
if (restaurant == null)
    throw new KeyNotFoundException($"Restaurant with ID {id} not found");
```

### External API Errors
```csharp
// In GeolocationAdapter:
if (!response.IsSuccessStatusCode)
    throw new GeolocationApiException(
        "API error",
        response.StatusCode,
        content
    );
```

### HTTP Response Mapping
```csharp
// In Endpoints:
try {
    var result = await mediator.Send(query);
    return Results.Ok(result);
} 
catch (ArgumentException ex) {
    return Results.BadRequest(new { error = ex.Message });
}
catch (KeyNotFoundException ex) {
    return Results.NotFound(new { error = ex.Message });
}
catch (Exception ex) {
    return Results.StatusCode(500, new { error = "Internal error" });
}
```

---

## 🚀 How to Run

### Development Environment

```bash
cd TechAcademyApi

# Option 1: Using run script
./run.sh          # Linux/Mac
run.bat           # Windows

# Option 2: Manual
dotnet restore    # Download dependencies
dotnet build      # Compile
dotnet run        # Start server

# Server starts on:
# HTTP:  http://localhost:5000
# HTTPS: https://localhost:5001
```

### Database
- Auto-initialized on startup
- SQLite file: `techacademy.db`
- Migrations auto-applied

### Testing Endpoints

```bash
# Terminal 1: Start server
dotnet run

# Terminal 2: Test endpoints
curl http://localhost:5000/restaurants
curl -X POST http://localhost:5000/restaurants \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","city":"NYC"}'

# Or use Swagger UI (if configured):
https://localhost:5001/swagger
```

---

## 📚 Project Structure Summary

```
TechAcademyApi/                     (Presentation/API)
├── Program.cs                      (Entry point)
├── Endpoints/RestaurantEndpoints.cs (Route handlers)
├── Properties/launchSettings.json
├── appsettings.json

TechAcademyApi.Core/                (Domain - Independence Layer)
├── Entities/
│   ├── Restaurant.cs
│   └── Review.cs
└── Abstractions/
    ├── IRepository.cs
    ├── IRestaurantRepository.cs
    └── IReviewRepository.cs

TechAcademyApi.Application/         (Use Cases/CQRS)
├── Features/Restaurants/
│   ├── Queries/
│   │   ├── GetAllRestaurantsQuery.cs
│   │   ├── GetRestaurantByIdQuery.cs
│   │   └── GetRestaurantsWithReviewsQuery.cs
│   ├── Commands/
│   │   ├── CreateRestaurantCommand.cs
│   │   ├── UpdateRestaurantCommand.cs
│   │   └── DeleteRestaurantCommand.cs
│   └── Handlers/
│       ├── RestaurantQueryHandlers.cs
│       └── RestaurantCommandHandlers.cs
├── DTOs/
│   └── RestaurantDto.cs
└── Abstractions/
    ├── IGeolocationAdapter.cs
    └── GeolocationApiException.cs

TechAcademyApi.Infrastructure/      (Data Access & External APIs)
├── Persistence/
│   ├── TechAcademyDbContext.cs
│   ├── Repositories/
│   │   ├── GenericRepository.cs
│   │   ├── RestaurantRepository.cs
│   │   └── ReviewRepository.cs
│   └── Migrations/
├── ExternalServices/
│   └── GeolocationAdapter.cs
└── DependencyInjectionExtensions.cs

TechAcademyApi.DependencyInjection/ (Central DI Registration)
└── ServiceCollectionExtensions.cs
```

---

## 🎓 Learning Outcomes

This solution demonstrates:

✅ **Clean Architecture** - Layered approach with clear separation  
✅ **CQRS Pattern** - Queries vs Commands for clarity  
✅ **Repository Pattern** - Data access abstraction  
✅ **Dependency Injection** - Loose coupling & testability  
✅ **Adapter Pattern** - External API abstraction  
✅ **DTO Pattern** - API contract decoupling  
✅ **Entity Framework Core** - Modern ORM usage  
✅ **MediatR** - In-process messaging  
✅ **Error Handling** - Comprehensive exception strategies  
✅ **Async/Await** - Non-blocking operations  
✅ **Validation** - Input and business logic validation  
✅ **Logging** - Debug and error tracking  

---

## 🔗 External Resources

- **Open-Meteo APIs** (Free geolocation & weather)
  - Geocoding: https://geocoding-api.open-meteo.com/v1/search
  - Weather: https://api.open-meteo.com/v1/forecast

- **ASP.NET Core** - https://learn.microsoft.com/aspnet

- **Entity Framework Core** - https://learn.microsoft.com/ef

- **MediatR** - https://github.com/jbogard/MediatR

- **Minimal APIs** - https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis

---

**Last Updated:** April 8, 2026
