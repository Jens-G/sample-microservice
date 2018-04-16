using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB.Cassandra
{
    public static class Tools
    {
        public static string EscapeValue(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        public static string EscapeValue(int value)
        {
            return value.ToString();
        }

        public static string EscapeValue(long value)
        {
            return value.ToString();
        }

        public static ISession Connect(IEnumerable<string> servers)
        {
            var builder = Cluster.Builder();
            foreach (var server in servers)
                builder.AddContactPoint(server);

            var queryOptions = new QueryOptions();
            queryOptions.SetConsistencyLevel(ConsistencyLevel.LocalQuorum);
            builder.WithQueryOptions(queryOptions);

            var cluster = builder.Build();
            return cluster.Connect();
        }



    }
}
