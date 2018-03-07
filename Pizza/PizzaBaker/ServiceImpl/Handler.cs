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
using ThriftClients;

namespace PizzaBaker.ServiceImpl
{
    class Handler 
    {
        static object ServerLock = new object();
        static string InstanceID = Guid.NewGuid().ToString();

        internal bool MakeOnePizza(PizzeriaCallbackClient client)
        {
            try
            {
                var work = client.Impl.GetSomeWork(GetID());
                if ((string.IsNullOrEmpty(work.OrderID)) || (work.OrderPosition == null))
                    return false;

                PrepareMeal(work);

                client.Impl.MealPrepared(
                        work.OrderID,
                        work.OrderPosition.DishID,
                        work.OrderPosition.Quantity,
                        InstanceID);

                return true;
            }
            catch (EPizzeria ce)
            {
                Console.WriteLine(ce.Message);
                Console.WriteLine(ce.Msg);
                //Console.WriteLine(ce.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} {1}", e.GetType().Name, e.Message);
                //Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        private bool PrepareMeal(WorkItem work)
        {
            lock (ServerLock)
            {
                Console.Write("Baking {0} {1} for {2} ... ", 
                    work.OrderPosition.Quantity,
                    work.OrderPosition.DishID,
                    work.OrderID);

                // do some hard work
                Thread.Sleep(work.OrderPosition.Quantity * 500);

                Console.WriteLine("done.");
                return true;
            }
        }

        public string GetID()
        {
            return InstanceID;
        }
    }

}
