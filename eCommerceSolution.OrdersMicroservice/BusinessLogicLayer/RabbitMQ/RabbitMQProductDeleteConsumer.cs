using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Azure.Amqp;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteConsumer : IDisposable, IRabbitMQProductDeleteConsumer
{
    private readonly IConfiguration _configuration;
    private  IChannel _channel;
    private  IConnection _connection;
    private readonly ILogger<RabbitMQProductDeleteConsumer> _logger;
    private readonly IDistributedCache _distributedCache;

    public RabbitMQProductDeleteConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeleteConsumer> logger, IDistributedCache distributedCache)
    {
        _configuration = configuration;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    private async Task SetupConnectionAndChannel()
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
        _connection = await connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task Initialize(int delayMilliseconds = 3000)
    {
        bool connected = false;
        int attempt = 0;
        while (!connected)
        {
            try
            {
                await SetupConnectionAndChannel();
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

    public async Task Consume()
    {
        //string routingKey = "product.delete";

        var headers = new Dictionary<string, object>()
            {
                {"x-match", "all" },
                {"event", "product.delete" },
                {"RowCount", 1 }
            };

        string queueName = "orders.product.delete.queue";

        //Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
       await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        //Create message queue
        await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        //Bind the message to exchange
        await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments:headers);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            if (message != null)
            {
                var productDTO = JsonSerializer.Deserialize<ProductDTO>(message);

                _logger.LogInformation($"Product deleted: {productDTO.ProductID}, Product Name: {productDTO.ProductName}");

                await HandleDelete(productDTO.ProductID);
            }

        };

        await _channel.BasicConsumeAsync(queue: queueName, consumer: consumer, autoAck: true);
    }

    private async Task HandleDelete(Guid productID)
    {
        string cacheKey = $"product:{productID}";
        await _distributedCache.RemoveAsync(cacheKey);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
