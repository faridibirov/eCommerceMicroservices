namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductDeleteConsumer
    {
        void Initialize(int delayMilliseconds);
        void Consume();
        void Dispose();
    }
}