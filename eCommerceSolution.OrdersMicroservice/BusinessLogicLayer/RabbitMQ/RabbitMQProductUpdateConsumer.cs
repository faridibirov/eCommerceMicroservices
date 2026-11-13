using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductUpdateConsumer : IDisposable, IRabbitMQProductUpdateConsumer
{
    private readonly IConfiguration _configuration;
    private IModel _channel;
    private IConnection _connection;
    private readonly ILogger<RabbitMQProductUpdateConsumer> _logger;
    private readonly IDistributedCache _distributedCache;

    public RabbitMQProductUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductUpdateConsumer> logger, IDistributedCache distributedCache)
    {
        _configuration = configuration;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    private void SetupConnectionAndChannel()
    {
        Console.WriteLine($"RabbitMQ_HostName: {_configuration["RabbitMQ_HostName"]}");
        Console.WriteLine($"RabbitMQ_UserName: {_configuration["RabbitMQ_UserName"]}");
        Console.WriteLine($"RabbitMQ_Password: {_configuration["RabbitMQ_Password"]}");
        Console.WriteLine($"RabbitMQ_ConnectionPort: {_configuration["RabbitMQ_ConnectionPort"]}");

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_ConnectionPort"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Initialize(int delayMilliseconds = 3000)
    {
        bool connected = false;
        int attempt = 0;
        while (!connected)
        {
            try
            {
                SetupConnectionAndChannel();
                connected = true;
                _logger.LogInformation("RabbitMQ Connected.");
            }
            catch (Exception ex)
            {
                attempt++;
                _logger.LogError(ex, $"Connection failed with RabbitMQ. Retrying in {delayMilliseconds} ms.");
                Thread.Sleep(delayMilliseconds);
            }
        }
    }

    public void Consume()
    {
        // string routingKey = "product.update.name";
        var headers = new Dictionary<string, object>()
            {
                {"x-match", "all" },
                {"event", "product.update" },
                {"RowCount", 1 }
            };

        string queueName = "orders.product.update.name.queue";

        //Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        //Create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        //Bind the message to exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            if (message != null)
            {
                ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(message);
                if (productDTO != null)
                {
                    await HandleUpdate(productDTO);
                }
            }

        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }


    private async Task HandleUpdate(ProductDTO productDTO)
    {
        string productJson = JsonSerializer.Serialize(productDTO);

        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
             .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

        string cacheKeyToWrite = $"product:{productDTO.ProductID}";
        _logger.LogInformation($"Product name updated: {productDTO.ProductID}, New name: {productDTO.ProductName}");

        await _distributedCache.SetStringAsync(cacheKeyToWrite, productJson, options);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
