using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pizzeria.ServiceImpl;
using CommonServiceTools;

namespace Pizzeria
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = args.Length > 0 ? int.Parse(args[0]) : PizzaConfig.Ports.Pizzeria;

            var maitre = new Task(() => {
                MaitreDeCuisine.Run();
            });
            maitre.Start();

            Console.WriteLine("Up and running on port {0}.", port);
            Server.Run(port);

            MaitreDeCuisine.Stop();
            maitre.Wait();
        }
    }
}
