using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Subsriber;

class Program
{
    static void Main(string[] args)
    {
        // Standart Subsriber - Consumer kullanımı
        // BasicExample example = new BasicExample();
        // example.Standart();

        ExchangeTypes exchange = new ExchangeTypes();
        // exchange.Fanout();
        // exchange.Direct();
        // exchange.Topic();
        // exchange.Header();
    }
}
