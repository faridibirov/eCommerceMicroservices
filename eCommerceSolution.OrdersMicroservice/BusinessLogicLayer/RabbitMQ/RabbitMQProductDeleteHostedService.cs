using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteHostedService : IHostedService
{
    private readonly IRabbitMQProductDeleteConsumer _consumer;

    public RabbitMQProductDeleteHostedService(IRabbitMQProductDeleteConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _consumer.Initialize(4000);
        await _consumer.Consume();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Dispose();
        return Task.CompletedTask;
    }

}

