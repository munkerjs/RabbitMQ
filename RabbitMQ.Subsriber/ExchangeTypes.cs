using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Subsriber
{
    public class ExchangeTypes
    {
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

            // Tüneldeki verileri okumak için consumer, subsriber oluşturalım
            var consumer = new EventingBasicConsumer(channel);

            // Verileri gönderdiğimiz, okuyacağımız Exchange Adı
            string exchangName = "logs-fanout";

            // Random kuyruk adı oluşturalım, birden fazla kuyruk yapmalıyız.
            var randomQueueName = channel.QueueDeclare().QueueName;

            // İlgili subsriber, instance kapansa dahi kuyruğun durmasını istiyorsanız aşağıdaki şekilde oluşturabilirsiniz.
            // channel.QueueDeclare(randomQueueName, true, false, false);

            // Kuyruk isimlerini Bind edelim.
            channel.QueueBind(randomQueueName, exchangName, "" , null);

            // RabbitMQ'dan mesajları kaçar kaçar alacağız, her bir Subsriber'a kaç mesaj ileteceğimizi belirteceğiz.
            // [Parametre 1] Boyut
            // [Parametre 2] Mesaj Sayısı
            // [Parametre 3][True]  Global, Kaç tane subsriber varsa tek seferde tüm subsriberların mesaj sayısı kadar çeker ve aralarında bölüşür. Örneğin; 3 ona 2 diğerine..
            // [Parametre 3][False] Global, kaç tane subsriber varsa tek seferde mesaj sayısı kadar gönderim sağlar. Örneğin; 5 ona 5 buna..
            channel.BasicQos(0, 1, false);

            // Tüketilecek kuyruğu seçelim.
            channel.BasicConsume(randomQueueName, false, consumer);

            Console.WriteLine("Loglar Dinleniyor..");

            // Subsriber'a mesaj geldiğinde bu event otomatik tetiklenecek.
            consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                // 1.5 Saniyelik Gecikme Verelim
                Thread.Sleep(1500);

                Console.WriteLine($"Gelen Mesaj: {message}");

                // Mesajları işledikten sonra silelim.
                // [True] İşlenmiş ama RabbitMQ'ya gitmemiş başka mesajlar varsa onun bilgilerini de RabbitMQ'ya haberdar eder.
                // [False] İlgili mesajın durumunu RabbitMQ'ya bildir.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

        /// <summary>
        /// Gelen mesajın root adına göre kuyruklara yönlendirir.
        /// Direkt olarak bir kuyruğu hedef alır.
        /// </summary>
        public void Direct()
        {
            /*
             * Örnek Senaryo
             * Rasgele log seviyelerine göre mesajlar gönderelim.
             * info message
             * warning message
             * error message
             * critical message
             * gibi ..
             */

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

            // Tüneldeki verileri okumak için consumer, subsriber oluşturalım
            var consumer = new EventingBasicConsumer(channel);

            // RabbitMQ'dan mesajları kaçar kaçar alacağız, her bir Subsriber'a kaç mesaj ileteceğimizi belirteceğiz.
            // [Parametre 1] Boyut
            // [Parametre 2] Mesaj Sayısı
            // [Parametre 3][True]  Global, Kaç tane subsriber varsa tek seferde tüm subsriberların mesaj sayısı kadar çeker ve aralarında bölüşür. Örneğin; 3 ona 2 diğerine..
            // [Parametre 3][False] Global, kaç tane subsriber varsa tek seferde mesaj sayısı kadar gönderim sağlar. Örneğin; 5 ona 5 buna..
            channel.BasicQos(0, 1, false);

            // Tüketilecek kuyruğu seçelim.
            // Oluşturulan Kuyruk İsimleri;
            // direct-queue-Critical
            // direct-queue-Error
            // direct-queue-Info
            // direct-queue-Warning
            // direct-queue-Success

            string queueName = "direct-queue-Critical";
            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Loglar Dinleniyor..");

            // Subsriber'a mesaj geldiğinde bu event otomatik tetiklenecek.
            consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                // 1.5 Saniyelik Gecikme Verelim
                Thread.Sleep(1500);

                Console.WriteLine($"Gelen Mesaj: {message}");

                // Örneğin; TXT Dosyası oluşturalım ve yazalım.
                // File.AppendAllText($"log-text.txt", message + "\n");

                // Mesajları işledikten sonra silelim.
                // [True] İşlenmiş ama RabbitMQ'ya gitmemiş başka mesajlar varsa onun bilgilerini de RabbitMQ'ya haberdar eder.
                // [False] İlgili mesajın durumunu RabbitMQ'ya bildir.
                channel.BasicAck(e.DeliveryTag, false);
            };

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

            // Tüneldeki verileri okumak için consumer, subsriber oluşturalım
            var consumer = new EventingBasicConsumer(channel);

            // RabbitMQ'dan mesajları kaçar kaçar alacağız, her bir Subsriber'a kaç mesaj ileteceğimizi belirteceğiz.
            // [Parametre 1] Boyut
            // [Parametre 2] Mesaj Sayısı
            // [Parametre 3][True]  Global, Kaç tane subsriber varsa tek seferde tüm subsriberların mesaj sayısı kadar çeker ve aralarında bölüşür. Örneğin; 3 ona 2 diğerine..
            // [Parametre 3][False] Global, kaç tane subsriber varsa tek seferde mesaj sayısı kadar gönderim sağlar. Örneğin; 5 ona 5 buna..
            channel.BasicQos(0, 1, false);

            // Tüketilecek kuyruğu seçelim.
            // Oluşturulan Kuyruk İsimleri;
            // direct-queue-Critical
            // direct-queue-Error
            // direct-queue-Info
            // direct-queue-Warning
            // direct-queue-Success

            // Mesajların boşa düşmemesi için önce bir kuyruk oluşturalım.
            string exchangName = "logs-topic";

            // Route Key 
            // string routeKey = $"*.Error.*";
            string routeKey = $"*.*.Warning";

            // Random Kuyruk Adı
            string queueName = channel.QueueDeclare().QueueName;

            // Bind Edelim
            channel.QueueBind(queueName, exchangName, routeKey, null);

            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Loglar Dinleniyor..");

            // Subsriber'a mesaj geldiğinde bu event otomatik tetiklenecek.
            consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                // 1.5 Saniyelik Gecikme Verelim
                Thread.Sleep(1500);

                Console.WriteLine($"Gelen Mesaj: {message}");

                // Örneğin; TXT Dosyası oluşturalım ve yazalım.
                // File.AppendAllText($"log-text.txt", message + "\n");

                // Mesajları işledikten sonra silelim.
                // [True] İşlenmiş ama RabbitMQ'ya gitmemiş başka mesajlar varsa onun bilgilerini de RabbitMQ'ya haberdar eder.
                // [False] İlgili mesajın durumunu RabbitMQ'ya bildir.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }
    }
}
