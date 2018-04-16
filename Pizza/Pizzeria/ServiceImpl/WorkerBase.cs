using CommonServiceTools;
using System.Collections.Generic;
using DB;
using System.Diagnostics;

namespace Pizzeria.ServiceImpl
{
    class WorkerBase
    {
        protected static IDbAdapter g_Adapter;
        protected static IDbSession g_Session;

        protected static IDbSession Session
        {
            get
            {
                if (g_Session != null)
                    return g_Session;

                switch (PizzaConfig.DBType)
                {
                    case DBType.Cassandra:
                        g_Adapter = new DB.Cassandra.CassandraAdapter();
                        g_Session = g_Adapter.CreateSession();
                        break;

                    case DBType.SQLServer:
                        g_Adapter = new DB.SQLServer.SQLServerAdapter();
                        g_Session = g_Adapter.CreateSession();
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }


                g_Session.EnsureKeyspaceAndTables();
                return g_Session;
            }
        }


    }
}
