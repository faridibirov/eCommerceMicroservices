namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public interface IServiceBusOrderPlacedConsumer : IDisposable
{ 
    Task ConsumeAsync();
}
