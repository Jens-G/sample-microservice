using System;
using System.Collections.Generic;
using System.Linq;
using DB;
using CommonServiceTools;
using Pizzeria;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DB.SQLServer
{
    public class SQLServerAdapter : IDbAdapter
    {
        public IDbSession CreateSession()
        {
            return new SQLServerSession();
        }
    }


    class SQLServerSession : IDbSession
    {
        

        private static SqlConnection Connect(bool setCatalog = true)
        {
            var connect = new List<string>();
            connect.Add(@"Data Source=" + PizzaConfig.Hosts.SQLServer);
            //connect.Add(@"Integrated Security=True");
            connect.Add(@"User ID=" + PizzaConfig.DBUser);
            connect.Add(@"Password=" + PizzaConfig.DBPassword);

            if (setCatalog)
                connect.Add(@"Initial Catalog=Pizzeria");

            var connection = new SqlConnection(string.Join(";", connect));
            connection.Open();
            return connection;
        }


        private void EnsureDababase()
        {
            try
            {
                using (var Connection = Connect(false))
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE DATABASE [Pizzeria];";
                    var rows = cmd.ExecuteNonQuery();
                }

            }
            catch (SqlException e)
            {
                Debug.Assert(e.Number == 1801, e.Message);
            }
        }


        private void EnsureTables()
        {
            try
            {
                using (var Connection = Connect())
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE [PendingOrders] ("
                                    + " OrderID varchar(39),"
                                    + " DishID varchar(20),"
                                    + " Quantity int,"
                                    + " Status int,"
                                    + " BakerID varchar(39),"
                                    + " TTL bigint,"  // UTC timestamp
                                    + " Changed bigint,"  // UTC timestamp
                                    + " PRIMARY KEY (OrderID,DishID)"
                                    + ");"
                                    ;
                    cmd.ExecuteNonQuery();
                }

            }
            catch (SqlException e)
            {
                Debug.Assert(e.Number == 2714, e.Message);
            }
        }


        public void EnsureKeyspaceAndTables()
        {
            EnsureDababase();
            EnsureTables();
        }

        public string PlaceOrder(Order order)
        {
            var sID = Guid.NewGuid().ToString();

            using (var Connection = Connect())
            {
                foreach (var entry in order.Positions)
                {
                    using (var cmd = Connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO [PendingOrders] "
                                        + "(OrderID, DishID, Quantity, Status, Changed) "
                                        + "VALUES (@id, @dish, @quantity, @status, @now)";
                        cmd.Parameters.AddWithValue("id", sID);
                        cmd.Parameters.AddWithValue("dish", entry.DishID);
                        cmd.Parameters.AddWithValue("quantity", entry.Quantity);
                        cmd.Parameters.AddWithValue("status", 0);
                        cmd.Parameters.AddWithValue("now", DateTime.Now.ToFileTimeUtc());

                        Console.WriteLine(cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return sID;
        }


        private WorkItem FindWorkItem( SqlConnection Connection, string BakerID)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT OrderID, DishID, Quantity, Status"
                                + " FROM [PendingOrders]"
                                + " WHERE (Status = @status)"
                                + "   AND (Status = 0)"
                                + "   AND ((BakerID is NULL) or (TTL is NULL) or (TTL < @now))"
                                + ";";
                cmd.Parameters.AddWithValue("status", 0);
                cmd.Parameters.AddWithValue("now", DateTime.Now.ToFileTimeUtc());

                //Console.WriteLine(cmd.CommandText); --don't pollute the output with useless information
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var orderID = (string)reader[0];
                        var dishID = (string)reader[1];
                        var quantity = (int)reader[2];

                        return new WorkItem()
                        {
                            OrderID = orderID,
                            OrderPosition = new OrderPosition()
                            {
                                DishID = dishID,
                                Quantity = quantity
                            }
                        };
                    }
                }
            }

            return null;
        }


        public WorkItem GetSomeWork(string BakerID)
        {
            using (var Connection = Connect())
            {
                while (true)
                {
                    WorkItem work = FindWorkItem(Connection, BakerID);
                    if (null == work)
                        return new WorkItem();  // no null return, empty item instead

                    using (var cmd = Connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE [PendingOrders]"
                                        + " SET BakerID =  @baker,"
                                        + "     TTL = @ttl"
                                        + "     Changed = @now"
                                        + "   AND (DishID = @dish)"
                                        + "   AND (Status = 0)"
                                        + "   AND ((BakerID is NULL) or (TTL is NULL) or (TTL < @now))"
                                        + ";";
                        cmd.Parameters.AddWithValue("baker", BakerID);
                        cmd.Parameters.AddWithValue("order", work.OrderID);
                        cmd.Parameters.AddWithValue("dish", work.OrderPosition.DishID);
                        cmd.Parameters.AddWithValue("ttl", (DateTime.Now + TimeSpan.FromSeconds(60)).ToFileTimeUtc()); // baker timeout 
                        cmd.Parameters.AddWithValue("now", DateTime.Now.ToFileTimeUtc());

                        //Console.WriteLine(cmd.CommandText);  -- don't pollute output
                        var nAffected = cmd.ExecuteNonQuery();
                        var applied = (nAffected > 0);
                        if (applied)  // check if someone else was faster
                        {
                            Console.WriteLine("applied = {0}, order = {1}, dish = {2}, baker = {3}", 
                                applied, 
                                work.OrderID,
                                work.OrderPosition.DishID,
                                BakerID);
                            return work; // we got that row
                        }
                    }
                }
            }
        }


        public void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID)
        {
            using (var Connection = Connect())
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE [PendingOrders]"
                                + " SET Status = 1,"
                                + "     BakerID = @baker,"
                                + "     TTL = 0"  // clear any active TTL
                                + "     Changed = @now"
                                + " WHERE (OrderID = @order)"
                                + "   AND (DishID = @dish)"
                                + "   AND (Status = 0)"
                                + ";";
                cmd.Parameters.AddWithValue("baker", BakerID);
                cmd.Parameters.AddWithValue("order", OrderID);
                cmd.Parameters.AddWithValue("dish", DishID);
                cmd.Parameters.AddWithValue("now", DateTime.Now.ToFileTimeUtc());

                Console.WriteLine(cmd.CommandText);
                cmd.ExecuteNonQuery();
            }
        }


        public bool CheckAndDeliver(string orderID)
        {
            // check order status of all dishes
            var count = 0;

            using (var Connection = Connect())
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Status FROM [PendingOrders]"
                                    + " WHERE (OrderID = @order)"
                                    + ";";
                    cmd.Parameters.AddWithValue("order", orderID);

                    Console.WriteLine(cmd.CommandText);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var status = Convert.ToInt32(reader[0]);
                            if (status == 0)
                                return false;  // still not complete
                            ++count;
                        }
                    }
                }


                // any records at all?
                if (count == 0)
                    throw new EPizzeria() { Msg = string.Format("Invalid orderID {0}", orderID) };

                // delivered, done, remove order
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM [PendingOrders]"
                                    + " WHERE (OrderID = @order) OR (Changed < @ancient)"
                                    + ";";
                    cmd.Parameters.AddWithValue("order", orderID);
                    cmd.Parameters.AddWithValue("ancient", (DateTime.Now - TimeSpan.FromDays(1)).ToFileTimeUtc()); // order timeout

                    Console.WriteLine(cmd);
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }


    }

}
