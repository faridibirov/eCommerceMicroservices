using Microsoft.Extensions.Hosting;

namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public class ServiceBusOrderPlacedHostedService : IHostedService
{
    private readonly IServiceBusOrderPlacedConsumer _consumer;

    public ServiceBusOrderPlacedHostedService(IServiceBusOrderPlacedConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
       await _consumer.ConsumeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Dispose();
        return Task.CompletedTask;
    }

}

