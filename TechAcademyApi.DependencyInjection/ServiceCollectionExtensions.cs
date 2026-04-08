using Microsoft.Extensions.DependencyInjection;
using TechAcademyApi.Application;
using TechAcademyApi.Infrastructure;

namespace TechAcademyApi.DependencyInjection
{
    /// <summary>
    /// Central Dependency Injection Orchestrator
    /// 
    /// This is the single entry point for DI configuration from the API layer.
    /// It coordinates the registration of all layers in the correct order:
    /// 
    /// 1. Application Layer (Use Cases, Services, Commands/Queries)
    /// 2. Infrastructure Layer (Data Access, DbContext, Repositories)
    /// 
    /// The Core layer has no dependencies and doesn't need DI registration.
    /// 
    /// Philosophy:
    /// - Each layer manages its own DI through extension methods
    /// - The API simply calls a single method: AddTechAcademyServices()
    /// - Dependencies flow inward (from API → Infrastructure → Application → Core)
    /// - No circular dependencies
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all TechAcademy API services to the DI container
        /// This is the ONLY method the API needs to call from Program.cs
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="connectionString">The database connection string</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTechAcademyServices(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddApplicationLayer();
            services.AddInfrastructureLayer(connectionString);

            return services;
        }
    }
}
