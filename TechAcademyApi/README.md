# TechAcademy API - Clean Architecture .NET 8 Web API

A production-ready ASP.NET Core 8 Web API built with clean architecture principles, SOLID design patterns, and industry best practices.

## 🏗️ Architecture Overview

This project demonstrates **Clean Architecture** with clear separation of concerns across multiple layers:

```
TechAcademyApi/
├── Models/               # Domain entities (Students, Courses)
├── Data/                 # Data access layer (DbContext)
├── Repositories/         # Repository pattern implementations
├── Interfaces/           # Service and repository abstractions
├── Services/             # Business logic layer
├── Endpoints/            # Minimal API endpoint handlers
├── Program.cs            # Application startup and DI configuration
├── appsettings.json      # Configuration
└── TechAcademyApi.csproj # Project file
```

## 🎯 SOLID Principles Applied

### 1. **Single Responsibility Principle (SRP)**
- Each class has a single responsibility:
  - `StudentRepository`: Data access for students
  - `StudentService`: Business logic for students
  - `StudentEndpoints`: HTTP endpoint handling

### 2. **Open/Closed Principle (OCP)**
- Code is open for extension via inheritance/interfaces:
  - `GenericRepository<T>`: Base implementation
  - `StudentRepository`: Extended specific functionality

### 3. **Liskov Substitution Principle (LSP)**
- Repositories and services can be substituted via interfaces:
  - `IRepository<T>` → `GenericRepository<T>`
  - `IStudentService` → `StudentService`

### 4. **Interface Segregation Principle (ISP)**
- Segregated interfaces:
  - `IRepository<T>`: Generic operations
  - `IStudentRepository`: Student-specific operations
  - `IStudentService`: Student business logic

### 5. **Dependency Inversion Principle (DIP)**
- High-level modules depend on abstractions:
  - `Services` depend on `IRepository` abstractions
  - Configured via IoC in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<IStudentService, StudentService>();
  builder.Services.AddScoped<IStudentRepository, StudentRepository>();
  ```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider

### Installation & Running

1. **Navigate to the project directory:**
   ```bash
   cd TechAcademyApi
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations:**
   ```bash
   dotnet ef database update
   ```
   (Migrations are also applied automatically on first run)

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **The API will be available at:**
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`
   - Health Check: `https://localhost:5001/health`

## 📚 API Endpoints

### Students
- `GET /api/students` - Get all students
- `GET /api/students/active` - Get active students
- `GET /api/students/{id}` - Get student by ID
- `GET /api/students/email/{email}` - Get student by email
- `POST /api/students` - Create new student
- `PUT /api/students/{id}` - Update student
- `DELETE /api/students/{id}` - Delete student

### Courses
- `GET /api/courses` - Get all courses
- `GET /api/courses/active` - Get active courses
- `GET /api/courses/{id}` - Get course by ID
- `GET /api/courses/code/{code}` - Get course by code
- `POST /api/courses` - Create new course
- `PUT /api/courses/{id}` - Update course
- `DELETE /api/courses/{id}` - Delete course

### Health
- `GET /health` - Health check endpoint

## 🗄️ Database

**Technology:** SQLite (local development)  
**Database File:** `techacademy.db`

### Connection String
Located in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=techacademy.db"
}
```

### Seeded Data
The database is automatically seeded with:
- 2 sample courses (C# Fundamentals, ASP.NET Core)
- 2 sample students (John Doe, Jane Smith)

## 📋 Request/Response Examples

### Create a Student
```bash
POST /api/students
Content-Type: application/json

{
  "firstName": "Alice",
  "lastName": "Johnson",
  "email": "alice.johnson@techacademy.com",
  "bio": "Full-stack developer enthusiast"
}
```

**Response (201 Created):**
```json
{
  "id": 3,
  "firstName": "Alice",
  "lastName": "Johnson",
  "email": "alice.johnson@techacademy.com",
  "bio": "Full-stack developer enthusiast",
  "enrollmentDate": "2026-04-07T12:00:00Z",
  "isActive": true
}
```

### Create a Course
```bash
POST /api/courses
Content-Type: application/json

{
  "title": "Advanced C# Patterns",
  "code": "CSH301",
  "description": "Master advanced design patterns in C#",
  "credits": 4
}
```

## 🔧 Configuration

### Environment Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=techacademy.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## 🏛️ Clean Architecture Layers

### **1. Domain Layer (Models)**
- `Student.cs` - Student domain entity
- `Course.cs` - Course domain entity
- Contains business rules and core domain logic

### **2. Data Access Layer (Data & Repositories)**
- `TechAcademyDbContext.cs` - Entity Framework DbContext
- `GenericRepository<T>` - Base repository implementation
- `StudentRepository` / `CourseRepository` - Specific repositories
- Abstracts database operations

### **3. Business Logic Layer (Services)**
- `StudentService` - Student business logic
- `CourseService` - Course business logic
- Validates data, applies business rules
- Orchestrates repository operations

### **4. Presentation Layer (Endpoints)**
- `StudentEndpoints.cs` - Student HTTP handlers
- `CourseEndpoints.cs` - Course HTTP handlers
- Maps HTTP requests to service calls
- Handles request/response translation

### **5. Interfaces (Abstractions)**
- `IRepository<T>` - Generic repository contract
- `IStudentRepository` / `ICourseRepository` - Specific repository contracts
- `IStudentService` / `ICourseService` - Service contracts
- Enable loose coupling and testability

## 💉 Dependency Injection

All dependencies are registered in `Program.cs`:

```csharp
// Register Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

// Register Services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

// Configure DbContext
builder.Services.AddDbContext<TechAcademyDbContext>(options =>
    options.UseSqlite(connectionString));
```

**Lifetimes:**
- **Scoped:** Repositories and Services (per HTTP request)
- **Singleton:** Configuration, Logger Factory
- **Transient:** One-time use objects

## 🧪 Testing Strategy

Services are designed for easy unit testing:
- All dependencies are injected via constructors
- Services depend on interfaces, not concrete implementations
- Mock repositories can be injected for testing

Example test setup:
```csharp
var mockRepository = new Mock<IStudentRepository>();
var logger = new Mock<ILogger<StudentService>>();
var service = new StudentService(mockRepository.Object, logger.Object);
```

## 📦 NuGet Dependencies

- `Microsoft.EntityFrameworkCore` - ORM
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite provider
- `Microsoft.EntityFrameworkCore.Design` - EF CLI tools

## 🔐 Error Handling

- Services validate inputs and throw meaningful exceptions
- Endpoints catch exceptions and return appropriate HTTP status codes
- All errors return JSON responses with descriptive messages

## 📝 Logging

Integrated logging at service level:
- Information logs for operations
- Configuration in `appsettings.json`
- Enable SQL query logging for debugging

## 🔄 Future Enhancements

- [ ] Add Unit Tests (xUnit)
- [ ] Implement Authentication (JWT)
- [ ] Add Authorization (Role-based)
- [ ] Implement Filtering, Sorting, Pagination
- [ ] Add Caching (Redis)
- [ ] Implement Logging (Serilog)
- [ ] Add API Versioning
- [ ] Configuration for SQL Server production database

## 📚 Best Practices Demonstrated

✅ Clean Architecture with clear layer separation  
✅ SOLID principles throughout  
✅ Repository pattern for data access  
✅ Dependency Injection via IoC container  
✅ Interface-based design  
✅ Async/await throughout  
✅ Validation at service layer  
✅ Meaningful exception handling  
✅ XML documentation comments  
✅ Minimal API for lightweight endpoints  
✅ Entity Framework Core best practices  
✅ Logging integration  

## 📄 License

This project is provided as-is for educational purposes.

---

**Ready to run:** `dotnet run` from the project directory and visit http://localhost:5000/swagger
