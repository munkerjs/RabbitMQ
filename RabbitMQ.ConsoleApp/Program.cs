using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;

class Program
{
    static void Main(string[] args)
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
        string queueName = "hello-queue";
        channel.QueueDeclare(queueName, true, false, false);

        #region Kuyruğa Tek Mesaj İletim Örneği
            //// Mesajımızı Oluşturalım.
            //string message = "Hello World!";

            //// RabbitMQ'ya verileri iletirken Byte dizisi şeklinde iletmekteyiz. PDF, Excel veya Image bile iletebilirsin.
            //var messageBody = Encoding.UTF8.GetBytes(message);

            //// Artık Mesajımızı Kuyruğa Ekleyelim.
            //channel.BasicPublish(string.Empty, queueName, null, messageBody);
            //Console.WriteLine("Mesajınızı Gönderildi.");
        #endregion

        #region Kuyruğa Birden Fazla Mesaj İletim Örneği
        Enumerable.Range(1, 50).ToList().ForEach(x=>
        {
            // Mesajımızı Oluşturalım.
            string message = $"Message {x}";

            // RabbitMQ'ya verileri iletirken Byte dizisi şeklinde iletmekteyiz. PDF, Excel veya Image bile iletebilirsin.
            var messageBody = Encoding.UTF8.GetBytes(message);

            // Artık Mesajımızı Kuyruğa Ekleyelim.
            channel.BasicPublish(string.Empty, queueName, null, messageBody);
            Console.WriteLine($"Mesajınızı Gönderilmiştir : {message}");
        });
        #endregion
               
        Console.ReadLine();
    }
}
