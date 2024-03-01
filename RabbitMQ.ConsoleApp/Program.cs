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
        // basic.SingleQueue();
        // basic.MultipleQueue();

        // Exchange Tipleri ile Kuruk Yönetimi
        ExchangeTypes exchange = new ExchangeTypes();
        // exchange.Fanout();
        // exchange.Direct();
        // exchange.Topic();
        exchange.Header();       
    }
}
