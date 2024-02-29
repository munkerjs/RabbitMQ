using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;
using RabbitMQ.Client.Events;

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

        // Tüneldeki verileri okumak için tüketici oluşturalım
        var consumer = new EventingBasicConsumer(channel);

        // Tüketicinin erişeceği, veriyi okuyacağı kuyruk adı
        string queueName = "hello-queue";

        // [EĞİTİM AMAÇLI SEÇİLİ] 2. değer true yapılırsa, mesaj doğruda işlense yanlış da işlense kuyruktan siler.
        // [GERÇEK PROJELERDE GENELDE KULLANIRIZ] 2. değer false yapılırsa, mesaj doğru işlenirse ben sana daha sonra silmen için bildireceğim gibi anlayabiliriz.
        channel.BasicConsume(queueName, true, consumer);

        // Subsriber'a mesaj geldiğinde bu event otomatik tetiklenecek.
        consumer.Received += (object? sender, BasicDeliverEventArgs e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"Gelen Mesaj: {message}");
        };

        Console.ReadLine();
    }
}
