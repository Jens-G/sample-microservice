using Pizzeria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;
using CommonServiceTools;

namespace Pizzeria.ServiceImpl
{
    class Handler : WorkerBase, Pizzeria.ISync, PizzeriaCallback.ISync, Diagnostics.Diagnostics.ISync
    {
        public Handler() : base()
        {
            Console.WriteLine("DBType = {0}", PizzaConfig.DBType);
        }


        public List<Dish> GetTheMenue()
        {
            try
            {
                Console.WriteLine("GetTheMenue() ...");
                return new List<Dish>() {
                    new Dish()
                    {
                        ID = "PIZ001",
                        Price = 8.95,
                        Description = "Pizza Quattro Stagioni, Empfehlung des Hauses",
                        Notes = "Unbedingt heiß genießen. Montags nicht im Angebot."
                    },
                    new Dish()
                    {
                        ID = "PIZ002",
                        Price = 10.00,
                        Description = "Pizza Tutto, für den goßen Hunger zwischendurch"
                    },
                    new Dish()
                    {
                        ID = "PIZ003",
                        Price = 6.50,
                        Description = "Pizza Funghi"
                    },
                    new Dish()
                    {
                        ID = "PIZ004",
                        Price = 7.50,
                        Description = "Pizza Margherita, der beliebte Klassiker"
                    },
                    new Dish()
                    {
                        ID = "PIZ005",
                        Price = 8.50,
                        Description = "Pizza Diavolo - heiß wie der Vesuv im Hochsommer",
                        Notes = "¼l Lambrusco incl."
                    }
                };
            }
            catch (Exception e)
            {
                throw Tools.RepackageException(e);
            }
        }


        public string PlaceOrder(Order order)
        {
            try
            {
                Console.WriteLine("PlaceOrder() ...");
                return Session.PlaceOrder(order);
            }
            catch (Exception e)
            {
                throw Tools.RepackageException(e);
            }
        }


        public WorkItem GetSomeWork(string BakerID)
        {
            try
            {
                Console.WriteLine("GetSomeWork() ...");
                return Session.GetSomeWork(BakerID);
            }
            catch (Exception e)
            {
                throw Tools.RepackageException(e);
            }
        }


        public void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID)
        {
            try
            {
                Console.WriteLine("MealPrepared() ...");
                Session.MealPrepared(OrderID, DishID, Quantity, BakerID);
            }
            catch (Exception e)
            {
                throw Tools.RepackageException(e);
            }
        }


        public bool CheckAndDeliver(string orderID)
        {
            try
            {
                Console.WriteLine("CheckAndDeliver() ...");
                return Session.CheckAndDeliver(orderID);
            }
            catch (Exception e)
            {
                throw Tools.RepackageException(e);
            }
        }


        public long PerformanceTest(int seconds)
        {
            try
            {
                double pi = 4;
                double diff = 1;
                double x = 3;
                bool plus = false;

                long iterations = 0;
                var timeout = DateTime.Now + TimeSpan.FromSeconds(seconds);
                while ((diff > 1e-300) && (timeout > DateTime.Now))
                {
                    diff = (4.0 / x);

                    if (plus)
                        pi += diff;
                    else
                        pi -= diff;

                    x += 2;
                    plus = (!plus);
                    ++iterations;
                }

                Console.WriteLine("pi = {0}, diff = {1}, iterations = {2}", pi, diff, iterations);
                return iterations;
            }
            catch (Exception e)
            {
                throw new Diagnostics.EDiagnostics() { Msg = e.Message };
            }
        }
    }

}
