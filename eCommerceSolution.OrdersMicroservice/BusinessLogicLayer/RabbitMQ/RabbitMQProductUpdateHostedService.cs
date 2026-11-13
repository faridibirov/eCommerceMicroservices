using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductUpdateHostedService : IHostedService
{
    private readonly IRabbitMQProductUpdateConsumer _consumer;

    public RabbitMQProductUpdateHostedService(IRabbitMQProductUpdateConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer.Initialize(4000);
        _consumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Dispose();
        return Task.CompletedTask;
    }

}

