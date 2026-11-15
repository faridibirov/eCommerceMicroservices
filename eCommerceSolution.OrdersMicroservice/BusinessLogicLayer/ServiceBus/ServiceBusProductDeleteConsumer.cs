using System.Text.Json;
using Azure.Messaging.ServiceBus;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public class ServiceBusProductDeleteConsumer : IServiceBusProductDeleteConsumer
{
    private readonly ServiceBusProcessor _serviceBusProcessor;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<ServiceBusProductDeleteConsumer> _logger;
    private readonly IConfiguration _configuration;

    public ServiceBusProductDeleteConsumer(IDistributedCache distributedCache,
        ILogger<ServiceBusProductDeleteConsumer> logger, ServiceBusClient serviceBusClient, IConfiguration configuration)
    {

        _distributedCache = distributedCache;
        _logger = logger;
        _configuration = configuration;
        _serviceBusProcessor = serviceBusClient.CreateProcessor(_configuration["ServiceBus:ServiceBus_Products_Delete_Topic"], _configuration["ServiceBus:ServiceBus_Products_Delete_Topic_Orders_Subscriptions"],
            new ServiceBusProcessorOptions()
            { AutoCompleteMessages = false });
        _serviceBusProcessor.ProcessMessageAsync += _serviceBusProcessor_ProcessMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += _serviceBusProcessor_ProcessErrorAsync;
    }

    private async Task _serviceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        string messageBodyJson = arg.Message.Body.ToString();
        ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(messageBodyJson);
        if (productDTO != null)
        {
            await HandleProductDeleting(productDTO);
        }
        //Complete the message to remove it from the queue
        await arg.CompleteMessageAsync(arg.Message);
    }

    public async Task HandleProductDeleting(ProductDTO productDTO)
    {
        string productJson = JsonSerializer.Serialize(productDTO);
        _logger.LogInformation($"ServiceBus: Product deleted: {productDTO.ProductID}, Product Name: {productDTO.ProductName}");

        string cacheKeyToDelete = $"product:{productDTO.ProductID}";
        await _distributedCache.RemoveAsync(cacheKeyToDelete);

    }

    private  Task _serviceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Error while handling message.");
        return Task.CompletedTask;
    }

    public async Task ConsumeAsync()
    {
        await _serviceBusProcessor.StartProcessingAsync();
    }


    public async void Dispose()
    {
        await _serviceBusProcessor.DisposeAsync();
    }


}
