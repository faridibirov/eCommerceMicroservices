namespace  eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public  interface IServiceBusPublisher
{
    Task Publish<T>(Dictionary<string, object> headers, T message);
}
