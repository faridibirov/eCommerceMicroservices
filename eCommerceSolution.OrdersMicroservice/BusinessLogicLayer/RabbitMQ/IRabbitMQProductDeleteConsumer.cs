namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductDeleteConsumer
    {
        Task Initialize(int delayMilliseconds);
        Task Consume();
        void Dispose();
    }
}