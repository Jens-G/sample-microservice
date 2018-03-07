using CommonServiceTools;
using Pizzeria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;
using ThriftClients;

namespace PizzaBaker.ServiceImpl
{
    class Server
    {
        static Handler handler = new Handler();

        internal static void Run()
        {
            Console.Title = Environment.MachineName + "-" + handler.GetID();

            while (true)
            {
                try
                {
                    Thread.Sleep(100);
                    using (var client = new ThriftClients.PizzeriaCallbackClient(PizzaConfig.Hosts.Pizzeria, PizzaConfig.Ports.Pizzeria))
                    {
                        while (handler.MakeOnePizza(client))
                            /* one more! */;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(1000); // no connection? be patient!
                }

            }
        }

    }
}
