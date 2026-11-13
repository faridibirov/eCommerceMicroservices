using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;
using Azure.Messaging.ServiceBus;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);
        services.AddScoped<IOrdersService, OrdersService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
        });

        services.AddTransient<IRabbitMQProductUpdateConsumer, RabbitMQProductUpdateConsumer>();
        services.AddTransient<IRabbitMQProductDeleteConsumer, RabbitMQProductDeleteConsumer>();
        services.AddHostedService<RabbitMQProductUpdateHostedService>();
        services.AddHostedService<RabbitMQProductDeleteHostedService>();

        //ServiceBus
        services.AddSingleton(_ => new ServiceBusClient(configuration["ServiceBus:ecommerce-servicebus-namespace"]));
        services.AddSingleton<IServiceBusProductUpdateConsumer, ServiceBusProductUpdateConsumer>();
        services.AddHostedService<ServiceBusProductUpdateHostedService>();

        return services;
    }
}
