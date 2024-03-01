using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Publisher
{
    public class ExchangeTypes
    {
        /// <summary>
        /// Fanout, herhangi bir filtre olmadan kendisine bağlı olan tüm kuyruklara ilgili mesajı iletir.
        /// </summary>
        public void Fanout()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = builder.Build();

            string rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ") ?? "";

            // Bilgilerimizi tanımlayalım
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(rabbitMqConnectionString);

            // RabbitMQ için bağlantı açalım
            using var connection = factory.CreateConnection();

            // Bağlantı Tüneli - Kanalı Oluşturalım ve RabbitMQ ya bağlanalım.
            var channel = connection.CreateModel();

            // Mesajların boşa düşmemesi için önce bir kuyruk oluşturalım.
            string exchangName = "logs-fanout";

            // [durable:true] Uygulama restart atılsa bile fiziksel olarak kayıt edelim, silinmesin.
            // [durable:false] Uygulama restart atılırsa tüm exchangeler kaybolur.
            channel.ExchangeDeclare(exchangName, durable:true,  type:ExchangeType.Fanout);

            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                // Mesajımızı Oluşturalım.
                string message = $"Log Message {x}";

                // RabbitMQ'ya verileri iletirken Byte dizisi şeklinde iletmekteyiz. PDF, Excel veya Image bile iletebilirsin.
                var messageBody = Encoding.UTF8.GetBytes(message);

                // Artık Mesajımızı Kuyruğa Ekleyelim.
                channel.BasicPublish(exchangName, "", null, messageBody);
                Console.WriteLine($"Mesajınızı Gönderilmiştir : {message}");
            });
        }
    }
}
