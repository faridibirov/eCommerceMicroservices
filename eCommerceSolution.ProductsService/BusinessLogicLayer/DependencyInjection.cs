using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace eCommerce.ProductsService.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);

        services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();
        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();
        services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();

        //ServiceBus
        services.AddSingleton(_ => new ServiceBus(configuration["ServiceBus:ecommerce-servicebus-namespace"]));
        services.AddSingleTon<IServiceBusPublisher, ServiceBusPublisher>();

        return services;
    }
}
