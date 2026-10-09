using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {

        services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);

        services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();
        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();

        return services;
    }
}
