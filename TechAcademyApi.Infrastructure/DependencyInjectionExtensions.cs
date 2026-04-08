using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechAcademyApi.Application.Abstractions;
using TechAcademyApi.Core.Abstractions;
using TechAcademyApi.Infrastructure.ExternalServices;
using TechAcademyApi.Infrastructure.Persistence;
using TechAcademyApi.Infrastructure.Persistence.Repositories;

namespace TechAcademyApi.Infrastructure
{
    /// <summary>
    /// Infrastructure Layer Dependency Injection Extension
    /// Registers all infrastructure-level services (repositories, DbContext, etc.)
    /// 
    /// Each layer encapsulates its own DI configuration, making the system modular.
    /// Infrastructure layer manages data access concerns.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds Infrastructure layer services to the DI container
        /// </summary>
        public static IServiceCollection AddInfrastructureLayer(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<TechAcademyDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();

            services.AddHttpClient<IGeolocationAdapter, GeolocationAdapter>();

            return services;
        }
    }
}
