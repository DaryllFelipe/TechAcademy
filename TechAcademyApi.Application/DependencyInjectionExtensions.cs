using Microsoft.Extensions.DependencyInjection;
using TechAcademyApi.Core.Abstractions;

namespace TechAcademyApi.Application
{
    /// <summary>
    /// Application Layer Dependency Injection Extension
    /// Registers all application-level services, commands, queries, and handlers
    /// 
    /// This extension encapsulates the DI configuration for the Application layer,
    /// following the principle that each layer manages its own dependencies.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds Application layer services to the DI container
        /// </summary>
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddMediatR(config =>
                config.RegisterServicesFromAssemblies(typeof(DependencyInjectionExtensions).Assembly));

            return services;
        }
    }
}
