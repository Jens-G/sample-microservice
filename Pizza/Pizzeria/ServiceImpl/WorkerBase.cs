using Cassandra;
using CommonServiceTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;

namespace Pizzeria.ServiceImpl
{
    class WorkerBase
    {
        protected static ISession Connect()
        {
            var session = CassandraTools.Connect(new List<string>() {
					PizzaConfig.Hosts.Cassandra
				});

            EnsureKeyspaceAndTables(session);
            return session;
        }

        private static void EnsureKeyspaceAndTables(ISession session)
        {
            session.CreateKeyspaceIfNotExists("pizzeria"/*, replstrat*/);
            try
            {
                var sCmd = "CREATE TABLE pizzeria.PendingOrders ("
                         + " OrderID varchar, DishID varchar, Quantity int, Status int, BakerID varchar,"
                         + " PRIMARY KEY (OrderID,DishID)"
                         + ") WITH CLUSTERING ORDER BY (DishID ASC);"
                         ;

                session.Execute(sCmd);
            }
            catch (AlreadyExistsException)
            {
                // shit happens
            }
        }


        protected static TException RepackageException(Exception e)
        {
            if (e is TException)
                return e as TException;

            var msg = new List<string>();
            while (e != null)
            {
                msg.Add(e.Message);
                e = e.InnerException;
            }

            return new EPizzeria() { Msg = string.Join("\n", msg) };
        }

    }
}
