namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public RabbitMQPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_Port"]!;

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

    public void Publish<T>(string routingKey, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

        //Create exchange
        string exchangeName = "products.exchange";
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Dircet, durable: true);

        //PUblish message
        _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: messageBodyInBytes);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
