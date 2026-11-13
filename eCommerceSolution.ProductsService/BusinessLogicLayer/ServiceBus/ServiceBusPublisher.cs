namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;
    private readonly ServiceBusSender _sender;

    public ServiceBusPublisher(ServiceBusClient serviceBusClient, IConfiguration configuration)
    {
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
        _sender = _serviceBusClient.CreateSender(_configuration["ServiceBus:ServiceBus_Products_Topic"]);
    }



    public async Task Publish<T>(Dictionary<string, object> headers, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageJson);

        foreach (var header in headers)
        {
            serviceBusMessage.ApplicationProperties[header.Key] = header.Value;
        }

        await _sender.SendMessageAsync(serviceBusMessage);
    }
}
