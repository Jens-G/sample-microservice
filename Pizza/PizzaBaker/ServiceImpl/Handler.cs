using PizzaBaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;
using System.Threading;
using Pizzeria;
using CommonServiceTools;

namespace PizzaBaker.ServiceImpl
{
    class Handler : PizzaBaker.ISync
    {
        private static TException RepackageException(Exception e)
        {
            if (e is TException)
                return e as TException;

            var msg = new List<string>();
            while (e != null)
            {
                msg.Add(e.Message);
                e = e.InnerException;
            }

            return new EPizzaBaker() { Msg = string.Join("\n", msg) };
        }


        static object ServerLock = new object();
        static string InstanceID = Guid.NewGuid().ToString();
        static Task CurrentTask;

        public bool PrepareMeal(string OrderID, string DishID, int Quantity)
        {
            try
            {
                lock (ServerLock)
                {
                    if ((CurrentTask != null) && (!CurrentTask.IsCompleted))
                        return false;

                    Console.WriteLine("Baking {0} {1} for {2}", Quantity, DishID, OrderID);

                    CurrentTask = new Task(() => {
                        Thread.Sleep(Quantity * 500);
                        NotifyMealPrepared(OrderID, DishID, Quantity);
                    });

                    CurrentTask.Start();
                    return true;
                }
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }

        private void NotifyMealPrepared(string orderID, string dishID, int quantity)
        {
            try
            {
                using (var client = new ThriftClients.PizzeriaCallbackClient(PizzaConfig.Hosts.Pizzeria, PizzaConfig.Ports.Pizzeria))
                {
                    client.Impl.MealPrepared(orderID, dishID, quantity, InstanceID);
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
                //Console.WriteLine(e.StackTrace);
            }
        }

        public string GetID()
        {
            try
            {
                return InstanceID;
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }
    }

}
