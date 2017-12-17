using CommonServiceTools;
using System;
using System.Threading;
using System.Linq;
using Cassandra;

namespace Pizzeria.ServiceImpl
{
    internal class MaitreDeCuisine : WorkerBase
    {
        private static ManualResetEvent evStop = new ManualResetEvent(false);

        internal static void Run()
        {
            while (!evStop.WaitOne(500))
            {
                InstructPizzaBakers();
            }
        }

        internal static void Stop()
        {
            evStop.Set();
        }



        private static void InstructPizzaBakers()
        {
            try
            {
                var session = Connect();

                var sCmd = "SELECT OrderID, DishID, Quantity, Status FROM pizzeria.PendingOrders"
                         + " WHERE Status = " + CassandraTools.EscapeValue(0)
                         + " ALLOW FILTERING;"
                         ;

                Console.WriteLine(sCmd);
                var rows = session.Execute(sCmd);
                foreach (var row in rows)
                {
                    var orderID = (string)row[0];
                    var dishID = (string)row[1];
                    var quantity = (int)row[2];

                    if (evStop.WaitOne(0))
                        return;

                    using (var client = new ThriftClients.PizzaBakerClient(PizzaConfig.Hosts.PizzaBaker, PizzaConfig.Ports.PizzaBaker))
                    {
                        var bakerID = client.Impl.GetID();

                        sCmd = "UPDATE pizzeria.PendingOrders"
                             + " USING TTL 120"  // 2 minuten 
                             + " SET BakerID = " + CassandraTools.EscapeValue(bakerID)
                             + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                             + " AND (DishID = " + CassandraTools.EscapeValue(dishID) + ")"
                             + " IF BakerID = NULL"
                             + ";";  // 1 minute
                        Console.WriteLine(sCmd);
                        var applied = (bool)session.Execute(sCmd).First().First();  // first row, first col
                        Console.WriteLine("applied = {0}", applied);
                        if (!applied)  // someone else was faster
                            continue;

                        if (!client.Impl.PrepareMeal(orderID, dishID, quantity))
                        {
                            sCmd = "UPDATE pizzeria.PendingOrders"
                                 + " SET BakerID = NULL"
                                 + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                                 + " AND (DishID = " + CassandraTools.EscapeValue(dishID) + ")"
                                 + " IF BakerID = " + CassandraTools.EscapeValue(bakerID)
                                 + ";";
                            Console.WriteLine(sCmd);
                            session.Execute(sCmd);
                            break;
                        }
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
        }

        internal static void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID)
        {
            try
            {
                var session = Connect();
                
                var sCmd = "UPDATE pizzeria.PendingOrders USING TTL 0"
                         + " SET Status = 1, BakerID = " + CassandraTools.EscapeValue(BakerID)  // set bakerID again to override active TTL
                         + " WHERE (OrderID = " + CassandraTools.EscapeValue(OrderID) + ")"
                         + " AND (DishID = " + CassandraTools.EscapeValue(DishID) + ")"
                         + " IF Status = " + CassandraTools.EscapeValue(0)
                         + ";";
                Console.WriteLine(sCmd);
                session.Execute(sCmd);
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }
    }
}