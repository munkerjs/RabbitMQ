using RabbitMQ.Client;
using RabbitMQ.WatermarkApp.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.WatermarkApp.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitmqClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitmqClientService)
        {
            _rabbitmqClientService = rabbitmqClientService;
        }

        public void Publish(ProductImageCreatedEvent events)
        {
            var channel = _rabbitmqClientService.Connect();
            var bodyString = JsonSerializer.Serialize(events);
            var convertByte = Encoding.UTF8.GetBytes(bodyString);

            // Mesajımız memoryde durmayıp fiziksel olarak kayıt edilsin.
            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            channel.BasicPublish(exchange:RabbitMQClientService.ExchangeName, routingKey:RabbitMQClientService.RoutingWatermark, basicProperties:props, body:convertByte);
        }
    }
}
