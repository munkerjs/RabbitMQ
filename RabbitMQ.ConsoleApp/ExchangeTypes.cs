using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static RabbitMQ.Publisher.ExchangeTypes;

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

            Console.ReadLine();
        }

        /// <summary>
        /// Direct, kuyruk adına göre hedefleme, gönderim yapılır.
        /// </summary>
        public void Direct()
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
            string exchangName = "logs-direct";

            // [durable:true] Uygulama restart atılsa bile fiziksel olarak kayıt edelim, silinmesin.
            // [durable:false] Uygulama restart atılırsa tüm exchangeler kaybolur.
            channel.ExchangeDeclare(exchangName, durable: true, type: ExchangeType.Direct);


            /* -------------------------------------------------------------------------------------
             * Örnek Senaryo
             * Rasgele log seviyelerine göre mesajlar gönderelim.
             * info message
             * warning message
             * error message
             * critical message
             * gibi ..
             */

            Enum.GetNames(typeof(LogNames)).ToList().ForEach(x=>
            {
                var queueName = $"direct-queue-{x}";
                channel.QueueDeclare(queueName, true, false, false, null);

                // Root belirlenmesi gerekiyor, kuyruk tipine göre bind edilecek.
                var routeKey = $"route-{x}";

                // Kuyruk isimlerini Bind edelim.
                channel.QueueBind(queueName, exchangName, routeKey, null);
            });

            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames logName = (LogNames)new Random().Next(1,6); // dizi içerisinde random değer getirelim.

                // Mesajımızı Oluşturalım.
                string message = $"Log-type: {logName} : Message {x}";

                // RabbitMQ'ya verileri iletirken Byte dizisi şeklinde iletmekteyiz. PDF, Excel veya Image bile iletebilirsin.
                var messageBody = Encoding.UTF8.GetBytes(message);

                // Root belirlenmesi gerekiyor, kuyruk tipine göre bind edilecek.
                var routeKey = $"route-{logName}";

                // Artık Mesajımızı Kuyruğa Ekleyelim.
                channel.BasicPublish(exchangName, routeKey, null, messageBody);
                Console.WriteLine($"Log Gönderilmiştir : {message}");
            });

            Console.ReadLine();
        }

        /// <summary>
        /// Bir mesaj gönderirken, rootkey'de string ifade yazmak yerine noktalarla beraber ifadeler belirtiyoruz.
        /// Örneğin; Critical.Error.Warning
        /// RootKey Örnekleri
        /// 1. Critical.Error.Warning
        /// 2. *.Error.*
        /// 3. #.Error
        /// </summary>
        public void Topic()
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
            string exchangName = "logs-topic";

            // [durable:true] Uygulama restart atılsa bile fiziksel olarak kayıt edelim, silinmesin.
            // [durable:false] Uygulama restart atılırsa tüm exchangeler kaybolur.
            channel.ExchangeDeclare(exchangName, durable: true, type: ExchangeType.Topic);

            Random rand = new Random();

            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames logName = (LogNames)new Random().Next(1, 6); // dizi içerisinde random değer getirelim.

                LogNames log1 = (LogNames)rand.Next(1, 6);
                LogNames log2 = (LogNames)rand.Next(1, 6);
                LogNames log3 = (LogNames)rand.Next(1, 6);

                // Root belirlenmesi gerekiyor, kuyruk tipine göre bind edilecek.
                var routeKey = $"{log1}.{log2}.{log3}";

                // Mesajımızı Oluşturalım.
                string message = $"Log-type: {log1} - {log2} - {log3}";

                // RabbitMQ'ya verileri iletirken Byte dizisi şeklinde iletmekteyiz. PDF, Excel veya Image bile iletebilirsin.
                var messageBody = Encoding.UTF8.GetBytes(message);

                // Artık Mesajımızı Kuyruğa Ekleyelim.
                channel.BasicPublish(exchangName, routeKey, null, messageBody);
                Console.WriteLine($"Log Gönderilmiştir : {message}");
            });

            Console.ReadLine();
        }

        public void Header()
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
            string exchangName = "header-exchange";

            // [durable:true] Uygulama restart atılsa bile fiziksel olarak kayıt edelim, silinmesin.
            // [durable:false] Uygulama restart atılırsa tüm exchangeler kaybolur.
            channel.ExchangeDeclare(exchangName, durable: true, type: ExchangeType.Headers);

            // Mesajı Gönderelim
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "A4");

            var properties = channel.CreateBasicProperties();
            properties.Headers = headers;

            properties.Persistent = true; // MESAJLAR KALICI HALE GELİR.

            var prodcut = new ProductClass
            {
                Id = 1,
                Name = "Kalem",
                Price = 100,
                Stock = 20
            };

            // Seriliazed Product Message
            // Mesaj olarak büyük datalar göndermek çok da tercih edilen bir yöntem değil ama örnek olsun.
            var productJsonString = JsonSerializer.Serialize(prodcut);

            channel.BasicPublish(exchangName, String.Empty, properties, Encoding.UTF8.GetBytes(productJsonString));

            Console.WriteLine("Mesaj Gönderilmiştir");
            Console.ReadLine();
        }

        #region Utils
        public enum LogNames
        {
            Critical = 1,
            Error = 2,
            Warning = 3,
            Info = 4,
            Success = 5
        }
        #endregion
    }
}
