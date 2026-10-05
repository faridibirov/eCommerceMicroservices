using eCommerce.BusinessLogicLayer.DTO;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {

        services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);

        return services;
    }
}
