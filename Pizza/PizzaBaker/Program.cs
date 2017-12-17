using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaBaker.ServiceImpl;
using CommonServiceTools;

namespace PizzaBaker
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = args.Length > 0 ? int.Parse(args[0]) : PizzaConfig.Ports.PizzaBaker;

            Console.WriteLine("Up and running on port {0}.", port);
            Server.Run(port);
        }
    }
}
