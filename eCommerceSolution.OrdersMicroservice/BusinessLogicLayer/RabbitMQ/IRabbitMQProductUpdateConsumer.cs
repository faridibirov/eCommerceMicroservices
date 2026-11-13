namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductUpdateConsumer
    {
        void Initialize(int delayMilliseconds);
        void Consume();
        void Dispose();
    }
}