using Azure.Messaging.ServiceBus;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public class ServiceBusOrderPlacedConsumer : IServiceBusOrderPlacedConsumer
{
    private readonly ServiceBusProcessor _serviceBusProcessor;
    private readonly ILogger<ServiceBusOrderPlacedConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IProductsService _productsService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ServiceBusOrderPlacedConsumer(ILogger<ServiceBusOrderPlacedConsumer> logger, ServiceBusClient serviceBusClient, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceBusProcessor = serviceBusClient.CreateProcessor(_configuration["ServiceBus:ServiceBus_Orders_Placed_Topic"], _configuration["ServiceBus:ServiceBus_Orders_Placed_Products_Subscriptions"],
            new ServiceBusProcessorOptions()
            { AutoCompleteMessages = false });
        _serviceBusProcessor.ProcessMessageAsync += _serviceBusProcessor_ProcessMessageAsync;
        _serviceBusProcessor.ProcessErrorAsync += _serviceBusProcessor_ProcessErrorAsync;
        _serviceScopeFactory = serviceScopeFactory;
    }

    private async Task _serviceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        string messageBodyJson = arg.Message.Body.ToString();
        OrderResponse? orderResponse = JsonSerializer.Deserialize<OrderResponse>(messageBodyJson);
        if (orderResponse != null)
        {
            //Child scope
            using var scope = _serviceScopeFactory.CreateScope();
            var productsService = scope.ServiceProvider.GetRequiredService<IProductsService>();

            await HandleOrderPlacement(orderResponse, productsService);
        }
        //Complete the message to remove it from the queue
        await arg.CompleteMessageAsync(arg.Message);
    }

    public async Task HandleOrderPlacement(OrderResponse orderResponse, IProductsService productsService)
    {
        _logger.LogInformation($"ServiceBus: Order Placed: {orderResponse.OrderID}, Order Date: {orderResponse.OrderDate.ToLongDateString()} {orderResponse.OrderDate.ToLongTimeString()} ");
        foreach (var orderItemResponse in orderResponse.OrderItems)
        {
            ProductResponse? existingProduct = await productsService.GetProductByCondition(temp => temp.ProductID == orderItemResponse.ProductID);
            if (existingProduct != null)
            {
                var productUpdateRequest = new ProductUpdateRequest
                {
                    ProductID = existingProduct.ProductID,
                    ProductName = existingProduct.ProductName,
                    UnitPrice = existingProduct.UnitPrice,
                    Category = existingProduct.Category,
                    QuantityInStock = existingProduct.QuantityInStock - orderItemResponse.Quantity,
                };
               await productsService.UpdateProduct(productUpdateRequest);
            }
        }
    }

    private Task _serviceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
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
