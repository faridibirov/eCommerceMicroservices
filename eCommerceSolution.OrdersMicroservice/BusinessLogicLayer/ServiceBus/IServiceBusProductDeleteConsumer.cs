namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public interface IServiceBusProductDeleteConsumer: IDisposable
{ 
    Task ConsumeAsync();
}
