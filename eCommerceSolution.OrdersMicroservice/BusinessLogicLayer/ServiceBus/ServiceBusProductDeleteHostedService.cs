using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public class ServiceBusProductDeleteHostedService : IHostedService
{
    private readonly IServiceBusProductDeleteConsumer _consumer;

    public ServiceBusProductDeleteHostedService(IServiceBusProductDeleteConsumer consumer)
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

