using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;
using RabbitMQ.Publisher;

class Program
{
    static void Main(string[] args)
    {
        // Basit Kuyruk Yönetimi
        // BasicExample basic = new BasicExample();

        // Tekli olarak kuyruğa veri ekleme örneği
        // basic.SingleQueue();

        // Çoklu olarak kuyruğa veri ekleme örneği
        // basic.MultipleQueue();

        // Exchange Tipleri ile Kuruk Yönetimi
        ExchangeTypes exchange = new ExchangeTypes();
        exchange.Fanout();



        Console.ReadLine();
    }
}
