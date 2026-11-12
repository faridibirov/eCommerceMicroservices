using eCommerce.Core.ServiceContracts;
using eCommerce.Core.Services;
using eCommerce.Core.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace eCommerce.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration )
    {
        services.AddTransient<IUsersService, UsersService>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddScoped<GraphServiceClient>
            (provider=>
            {
                var scopes = new[] { "https://graph.microsoft.com/.default" };
                var options = new ClientSecretCredentialOptions()
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };
                var clientSecretCredential = new ClientSecretCredential(configuration["AzureAdB2c:TenantId"], configuration["AzureAdB2c:ClientId"], configuration["AzureAdB2c:ClientSecret"], options);
                var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
                return graphClient;
            });

        return services;
    }
}
