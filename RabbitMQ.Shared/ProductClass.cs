using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Shared
{
    public class ProductClass
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public  decimal Price { get; set; }
        public  int Stock { get; set; }
    }
}
