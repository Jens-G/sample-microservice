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
    class Handler : WorkerBase, Pizzeria.ISync, PizzeriaCallback.ISync
    {

        public List<Dish> GetTheMenue()
        {
            try
            {
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
                var session = Connect();

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
                    session.Execute(sCmd);

                }

                return sID;
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }


        public void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID)
        {
            MaitreDeCuisine.MealPrepared(OrderID, DishID, Quantity, BakerID);
        }

        public bool CheckAndDeliver(string orderID)
        {
            try
            {
                var session = Connect();

                // check order status of all dishes
                var count = 0;
                var sCmd = "SELECT Status FROM pizzeria.PendingOrders"
                         + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                         + ";";
                Console.WriteLine(sCmd);
                var rows = session.Execute(sCmd);
                foreach (var row in rows)
                {
                    var status = (int)row[0];
                    if (status == 0)
                        return false;  // still not complete
                    ++count;
                }


                // any records at all?
                if (count == 0)
                    throw new EPizzeria() { Msg = string.Format("Invalid orderID {0}", orderID) };

                // delivered, done, remove order
                sCmd = "DELETE FROM pizzeria.PendingOrders"
                     + " WHERE (OrderID = " + CassandraTools.EscapeValue(orderID) + ")"
                     + ";";
                Console.WriteLine(sCmd);
                session.Execute(sCmd);

                return true;
            }
            catch (Exception e)
            {
                throw RepackageException(e);
            }
        }

    }

}
