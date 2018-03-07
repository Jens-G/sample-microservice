using Pizzeria;
using System;
using System.Collections.Generic;
using Cassandra;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;
using CommonServiceTools;

namespace Pizzeria.ServiceImpl
{
    class Handler : WorkerBase, Pizzeria.ISync, PizzeriaCallback.ISync, Diagnostics.Diagnostics.ISync
    {
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
                throw RepackageException(e);
            }
        }


        public string PlaceOrder(Order order)
        {
            try
            {
                var sID = Guid.NewGuid().ToString();

                foreach (var entry in order.Positions)
                {
                    var sCmd = "INSERT INTO pizzeria.PendingOrders (OrderID, DishID, Quantity, Status)"
                                + " VALUES ("
                                + CassandraTools.EscapeValue(sID) + ","
                                + CassandraTools.EscapeValue(entry.DishID) + ","
                                + CassandraTools.EscapeValue(entry.Quantity) + ","
                                + CassandraTools.EscapeValue(0) + ");"
                                ;

                    Console.WriteLine(sCmd);
                    using (Session.Execute(sCmd)) { /* nix */ }

                }

                return sID;
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }


        public WorkItem GetSomeWork(string BakerID)
        {
            try
            {
                var sCmd = "SELECT OrderID, DishID, Quantity, Status FROM pizzeria.PendingOrders"
                        + " WHERE Status = " + CassandraTools.EscapeValue(0)
                        + " ALLOW FILTERING;"
                        ;

                Console.WriteLine(sCmd);
                using (var rows = Session.Execute(sCmd))
                {
                    foreach (var row in rows)
                    {
                        var orderID = (string)row[0];
                        var dishID = (string)row[1];
                        var quantity = (int)row[2];

                        var work = new WorkItem()
                        {
                            OrderID = orderID,
                            OrderPosition = new OrderPosition()
                            {
                                DishID = dishID,
                                Quantity = quantity
                            }
                        };

                        sCmd = "UPDATE pizzeria.PendingOrders"
                                + " USING TTL 60"  // seconds baker timeout
                                + " SET BakerID = " + CassandraTools.EscapeValue(BakerID)
                                + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                                + " AND (DishID = " + CassandraTools.EscapeValue(dishID) + ")"
                                + " IF BakerID = NULL"
                                + ";";
                        Console.WriteLine(sCmd);
                        using (var rowset = Session.Execute(sCmd))
                        {
                            var applied = (bool)rowset.First().First();  // first row, first col
                            Console.WriteLine("applied = {0}", applied);
                            if (applied)  // check if someone else was faster
                                return work; // no, we got that row
                        }
                    }
                }

                return new WorkItem();  // no null return, empty item instead
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }

        }


        public void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID)
        {
            try
            {
                var sCmd = "UPDATE pizzeria.PendingOrders USING TTL 0"
                        + " SET Status = 1, BakerID = " + CassandraTools.EscapeValue(BakerID)  // set bakerID again to override active TTL
                        + " WHERE (OrderID = " + CassandraTools.EscapeValue(OrderID) + ")"
                        + " AND (DishID = " + CassandraTools.EscapeValue(DishID) + ")"
                        + " IF Status = " + CassandraTools.EscapeValue(0)
                        + ";";
                Console.WriteLine(sCmd);
                using (Session.Execute(sCmd)) { /* nix */ }
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }


        public bool CheckAndDeliver(string orderID)
        {
            try
            {
                // check order status of all dishes
                var count = 0;
                var sCmd = "SELECT Status FROM pizzeria.PendingOrders"
                            + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                            + ";";
                Console.WriteLine(sCmd);
                using (var rows = Session.Execute(sCmd))
                {
                    foreach (var row in rows)
                    {
                        var status = (int)row[0];
                        if (status == 0)
                            return false;  // still not complete
                        ++count;
                    }
                }


                // any records at all?
                if (count == 0)
                    throw new EPizzeria() { Msg = string.Format("Invalid orderID {0}", orderID) };

                // delivered, done, remove order
                sCmd = "DELETE FROM pizzeria.PendingOrders"
                        + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                        + ";";
                Console.WriteLine(sCmd);
                using (Session.Execute(sCmd)) { /* nix */ }

                return true;
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }

        public double PerformanceTest(int seconds)
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
