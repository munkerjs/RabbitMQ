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

        // RabbitMQ'dan mesajları kaçar kaçar alacağız, her bir Subsriber'a kaç mesaj ileteceğimizi belirteceğiz.
        // [Parametre 1] Boyut
        // [Parametre 2] Mesaj Sayısı
        // [Parametre 3][True]  Global, Kaç tane subsriber varsa tek seferde tüm subsriberların mesaj sayısı kadar çeker ve aralarında bölüşür. Örneğin; 3 ona 2 diğerine..
        // [Parametre 3][False] Global, kaç tane subsriber varsa tek seferde mesaj sayısı kadar gönderim sağlar. Örneğin; 5 ona 5 buna..
        channel.BasicQos(0, 1, false);

        // Tüneldeki verileri okumak için tüketici oluşturalım
        var consumer = new EventingBasicConsumer(channel);

        // Tüketicinin erişeceği, veriyi okuyacağı kuyruk adı
        string queueName = "hello-queue";

        // [EĞİTİM AMAÇLI SEÇİLİ] 2. değer true yapılırsa, mesaj doğruda işlense yanlış da işlense kuyruktan siler.
        // [GERÇEK PROJELERDE GENELDE KULLANIRIZ] 2. değer false yapılırsa, mesaj doğru işlenirse ben sana daha sonra silmen için bildireceğim gibi anlayabiliriz.
        channel.BasicConsume(queueName, false, consumer);

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
}
