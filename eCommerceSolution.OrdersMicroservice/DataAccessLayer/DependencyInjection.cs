using eCommerce.DataAccessLayer.Context;
using eCommerce.DataAccessLayer.Repositories;
using eCommerce.DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrdersMicroservice.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;

        string connectionString = connectionStringTemplate.Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
                        .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));

        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

        services.AddScoped<IMongoDatabase>(proider=>{
           IMongoClient client = proider.GetRequiredService<IMongoClient>();
           return client.GetDatabase("OrdersDatabase");
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}
