using authentication_api.Data;
using authentication_api.Interfaces;
using authentication_api.Services;
using Microsoft.EntityFrameworkCore;

namespace authentication_api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            string connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<DataContext>(option =>
                option.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
