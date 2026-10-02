using eCommerce.DataAccessLayer.DbContext;

namespace eCommerceSolution.ProductsService.DataAccessLayer;

public class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbcontaxt>(options=>
        {
            options.UseMySQL(configuration.GetConnectionString("DefaultConnection")!);

        });

        return services;
    }
}
