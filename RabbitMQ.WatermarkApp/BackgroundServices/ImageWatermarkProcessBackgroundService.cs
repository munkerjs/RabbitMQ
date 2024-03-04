using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.WatermarkApp.Events;
using RabbitMQ.WatermarkApp.Services;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.WatermarkApp.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {

        private readonly RabbitMQClientService _clientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;

        private IModel _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService clientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _clientService.Connect();
            _channel.BasicQos(0,1,false); // birer birer alalım


            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {

            try
            {
                // Resme Watermark ekleyelim.
                var imageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageCreatedEvent.ImageName);
                var imagePath = Path.Combine(path, imageCreatedEvent.ImageName);
                var watermarkPath = Path.Combine(path, "watermarks");

                var siteName = "www.mysite.com";

                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);
                var font = new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(siteName, font);

                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);

                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                graphic.DrawString(siteName, font, brush, position); // artık çizebilirsin.

                var savePath = Path.Combine(watermarkPath, Path.GetFileName(imagePath)); // Dosya adını koruyarak yeni yol oluştur
                img.Save(savePath);

                // Bellek Boşaltalım
                img.Dispose();
                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
            }            

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
