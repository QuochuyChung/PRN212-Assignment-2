using asg2.DAL.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace asg2.DAL.Data
{
    /// <summary>
    /// CHỈ BLL được gọi hàm này (trong AddBusinessLayer). Tầng Web không nhìn thấy DAL.
    /// Tự đăng ký: mọi class tên *Repository có interface I*Repository, và mọi ISeeder trong assembly này.
    /// </summary>
    public static class DataAccessServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. Copy asg2.Web/appsettings.example.json to asg2.Web/appsettings.json.");

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            var implementations = typeof(AppDbContext).Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

            foreach (var impl in implementations)
            {
                if (impl.Name.EndsWith("Repository", StringComparison.Ordinal))
                {
                    var contract = impl.GetInterfaces().FirstOrDefault(i => i.Name == "I" + impl.Name);
                    if (contract is not null)
                        services.AddScoped(contract, impl);
                }
                else if (typeof(ISeeder).IsAssignableFrom(impl))
                {
                    services.AddScoped(typeof(ISeeder), impl);
                }
            }

            return services;
        }
    }
}
