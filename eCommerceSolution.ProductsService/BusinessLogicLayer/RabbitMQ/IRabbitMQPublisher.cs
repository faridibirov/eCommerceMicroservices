namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQPublisher
{
    Task Publish<T>(Dictionary<string, object> headers, T message);
    Task Initialize();
}
