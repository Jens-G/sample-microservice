using System;
using System.Collections.Generic;
using Cassandra;
using System.Linq;
using DB;
using CommonServiceTools;
using Pizzeria;

namespace DB.Cassandra
{
    public class CassandraAdapter : IDbAdapter
    {
        public IDbSession CreateSession()
        {
            return new CassandraSession();
        }
    }


    class CassandraSession : IDbSession
    { 
        private ISession Session;

        public CassandraSession()
        {
            Session = Tools.Connect(new List<string>() {
                PizzaConfig.Hosts.Cassandra
            });
        }

        public void EnsureKeyspaceAndTables()
        {
            Session.CreateKeyspaceIfNotExists("pizzeria"/*, replstrat*/);
            try
            {
                var sCmd = "CREATE TABLE pizzeria.PendingOrders ("
                         + " OrderID varchar, DishID varchar, Quantity int, Status int, BakerID varchar, Changed bigint,"
                         + " PRIMARY KEY (OrderID,DishID)"
                         + ") WITH CLUSTERING ORDER BY (DishID ASC);"
                         ;

                Session.Execute(sCmd);
            }
            catch (AlreadyExistsException)
            {
                // shit happens
            }
        }

        public string PlaceOrder(Order order)
        {
            // clean up the kitchen
            CleanupOldData();

            // create the order
            var sID = Guid.NewGuid().ToString();
            foreach (var entry in order.Positions)
            {
                var sCmd = "INSERT INTO pizzeria.PendingOrders (OrderID, DishID, Quantity, Status, Changed)"
                            + " VALUES ("
                            + Tools.EscapeValue(sID) + ","
                            + Tools.EscapeValue(entry.DishID) + ","
                            + Tools.EscapeValue(entry.Quantity) + ","
                            + Tools.EscapeValue(0) + ","
                            + Tools.EscapeValue(DateTime.Now.ToFileTimeUtc()) +");";

                Console.WriteLine(sCmd);
                using (Session.Execute(sCmd)) { /* nix */ }
            }

            return sID;
        }


        public WorkItem GetSomeWork(string BakerID)
        {
            var sCmd = "SELECT OrderID, DishID, Quantity, Status FROM pizzeria.PendingOrders"
                    + " WHERE Status = " + Tools.EscapeValue(0)
                    + " ALLOW FILTERING;"
                    ;

            //Console.WriteLine(sCmd);  -- don't pollute the output with useless information
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
                            + " SET BakerID = " + Tools.EscapeValue(BakerID)+", "
                            + "     Changed = " + Tools.EscapeValue(DateTime.Now.ToFileTimeUtc())
                            + " WHERE (OrderID = " + Tools.EscapeValue(orderID) + ")"
                            + " AND (DishID = " + Tools.EscapeValue(dishID) + ")"
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


        public void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID)
        {
            var sCmd = "UPDATE pizzeria.PendingOrders USING TTL 0"
                    + " SET Status = 1,"
                    + "    BakerID = " + Tools.EscapeValue(BakerID)+", " // set bakerID again to override active TTL
                    + "    Changed = " + Tools.EscapeValue(DateTime.Now.ToFileTimeUtc())
                    + " WHERE (OrderID = " + Tools.EscapeValue(OrderID) + ")"
                    + " AND (DishID = " + Tools.EscapeValue(DishID) + ")"
                    + " IF Status = " + Tools.EscapeValue(0)
                    + ";";
            Console.WriteLine(sCmd);
            using (Session.Execute(sCmd)) { /* nix */ }
        }


        private void CleanupOldData()
        {
            try
            {
                var orders = new HashSet<string>();

                // delivered, done, remove order
                var sCmd = "SELECT OrderID FROM pizzeria.PendingOrders"
                         + " WHERE (Changed < " + Tools.EscapeValue((DateTime.Now - TimeSpan.FromDays(1)).ToFileTimeUtc()) + ")"
                         + " ALLOW FILTERING;";
                Console.WriteLine(sCmd);
                using (var rows = Session.Execute(sCmd))
                {
                    foreach (var row in rows)
                    {
                        var orderID = (string)row[0];
                        if( !orders.Contains(orderID))
                            orders.Add(orderID);
                    }
                }

                // alle löschen
                foreach (var orderID in orders)
                    DeleteOrder(orderID);

            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup: {0}", e.Message);
            }
        }


        private void DeleteOrder(string orderID)
        {

            // delivered, done, remove order
            var sCmd = "DELETE FROM pizzeria.PendingOrders"
                        + " WHERE (OrderID = " + Tools.EscapeValue(orderID) + ")"
                        + ";";
            Console.WriteLine(sCmd);
            using (Session.Execute(sCmd)) { /* nix */ }
        }

        public bool CheckAndDeliver(string orderID)
        {
            // clean up the kitchen
            CleanupOldData();

            // check order status of all dishes
            var count = 0;
            var sCmd = "SELECT Status FROM pizzeria.PendingOrders"
                        + " WHERE (OrderID = " + Tools.EscapeValue(orderID) + ")"
                        + ";";
            //Console.WriteLine(sCmd);  -- don't pollute the output with useless information
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

            DeleteOrder(orderID);
            return true;
        }


    }

}
