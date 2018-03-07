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
                string server = "localhost";
                int port = PizzaConfig.Ports.Pizzeria;
                int clients = 1;
                bool perftest = false;
                bool silent = false;

                for (var i = 0; i < args.Length; ++i)
                {
                    int tmp;

                    switch (i)
                    {
                        case 0:
                            server = args[i];
                            break;
                        case 1:
                            if( int.TryParse(args[i], out tmp))
                                port = tmp;
                            break;
                        case 2:
                            if (int.TryParse(args[i], out tmp))
                            {
                                clients = Math.Max(1, Math.Min(128, tmp));
                                silent = true;
                            }
                            break;
                    }

                    if (string.CompareOrdinal(args[i], "--perftest") == 0)
                        perftest = true;
                }


                var start = DateTime.Now;

                var threads = new List<Thread>();
                for (var i = 0; i < clients; ++i)
                {
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            if (perftest)
                            {
                                using (var client = new ThriftClients.DiagnosticsClient(server, port, silent))
                                {
                                    var x = client.Impl.PerformanceTest(10);
                                    Console.WriteLine("Performance Test: {0} iterations",x);
                                }
                            }
                            else
                            {
                                using (var client = new ThriftClients.PizzeriaClient(server, port, silent))
                                {
                                    Run(client.Impl, silent);
                                }
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
                    });

                    thread.Start();
                    threads.Add(thread);
                }

                if (silent)
                    Console.WriteLine("{0} client threads started", clients);

                while (threads.Count > 0)
                {
                    foreach (var t in threads)
                    {
                        if (silent)
                            Console.Write("\r{0} client threads still running, {1} seconds so far    ", 
                                threads.Count, 
                                0.001 * (DateTime.Now - start).TotalMilliseconds);
                        if (t.Join(200))
                        {
                            threads.Remove(t);
                            break;
                        }
                    }
                }

                var delta = DateTime.Now - start;
                Console.WriteLine("\nCompleted after {0} seconds.", 0.001 * delta.TotalMilliseconds);
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


        private static void Run(Pizzeria.Pizzeria.ISync client, bool silent)
        {
            if (!silent)
            {
                Console.WriteLine("The MENUE\n------------");
                foreach (var dish in client.GetTheMenue())
                    Console.WriteLine("- {0} {1} nur {2} EUR {3}", dish.ID, dish.Description, dish.Price, dish.Notes);
                Console.WriteLine();
            }

            var fatfinger = new Random();
            var positions = new List<OrderPosition>();
            foreach (var dish in client.GetTheMenue())
            {
                var quantity = fatfinger.Next(8);
                if (quantity > 0)
                {
                    if (!silent)
                        Console.WriteLine("- ordering {0} of {1} {2}", quantity, dish.ID, dish.Description);
                    positions.Add(new OrderPosition()
                    {
                        DishID = dish.ID,
                        Quantity = quantity
                    });
                }
            }

            try
            {
                var orderID = client.PlaceOrder(new Order()
                {
                    Positions = positions
                });

                if (!silent)
                {
                    Console.WriteLine();
                    Console.WriteLine("Order {0} placed.", orderID);
                    Console.Write("Waiting ...");
                }

                while (!client.CheckAndDeliver(orderID))
                {
                    Thread.Sleep(1200);
                    if (!silent)
                        Console.Write(".");
                }

                if (!silent)
                {
                    Console.WriteLine(" Cool, my order {0} was finally delivered.", orderID);
                    Thread.Sleep(400);
                    Console.WriteLine("Mmmh, delicious!");
                }
            }
            catch (EPizzeria ep)
            {
                Console.WriteLine("Server error: " + ep.Msg);
            }
        }
    }
}
