using CommonServiceTools;
using Pizzeria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;


namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var server = args.Length > 0 ? args[0] : PizzaConfig.Hosts.Pizzeria;
                var port = args.Length > 1 ? int.Parse(args[1]) : PizzaConfig.Ports.Pizzeria;

                using (var client = new ThriftClients.PizzeriaClient(server, port))
                {
                    Run(client.Impl);
                }
            }
            catch (EPizzeria ce)
            {
                Console.WriteLine(ce.Message);
                Console.WriteLine(ce.Msg);
                //Console.WriteLine(ce.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} {1}", e.GetType().Name, e.Message);
            }
        }




        private static void Run(Pizzeria.Pizzeria.ISync client)
        {
            Console.WriteLine("The MENUE\n------------");
            foreach (var dish in client.GetTheMenue())
                Console.WriteLine("- {0} {1} nur {2} EUR {3}", dish.ID, dish.Description, dish.Price, dish.Notes);
            Console.WriteLine();


            var fatfinger = new Random();
            var positions = new List<OrderPosition>();
            foreach (var dish in client.GetTheMenue())
            {
                var quantity = fatfinger.Next(20);
                if (quantity > 0)
                {
                    Console.WriteLine("- ordering {0} of {1} {2}", quantity, dish.ID, dish.Description);
                    positions.Add(new OrderPosition()
                    {
                        DishID = dish.ID,
                        Quantity = quantity
                    });
                }
            }
            Console.WriteLine();

            try
            {
                var orderID = client.PlaceOrder(new Order()
                {
                    Positions = positions
                });

                Console.WriteLine("Order {0} placed.", orderID);

                Console.Write("Waiting ...");
                while (! client.CheckAndDeliver(orderID))
                {
                    Thread.Sleep(1200);
                    Console.Write(".");
                }
                Console.WriteLine(" Cool, my order {0} was finally delivered.", orderID);

                Thread.Sleep(400);
                Console.WriteLine("Mmmh, delicious!");
            }
            catch (EPizzeria ep)
            {
                Console.WriteLine("Server error: "+ep.Msg);
            }
        }
    }
}
