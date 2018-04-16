using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;

namespace CommonServiceTools
{
    public enum DBType
    {
        Cassandra,
        SQLServer
    }

    public static class PizzaConfig
    {
        private static Configuration Config = null;

        private static void OpenConfig()
        {
            if (Config == null)
            {
                try
                {
                    var dir = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                    var map = new ExeConfigurationFileMap()
                    {
                        ExeConfigFilename = Path.Combine(dir, "Hosts.config")
                    };

                    Config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                    //Console.WriteLine("OpenConfig successful: file {0}", dir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("OpenConfig failed: {0}", e.Message);
                }
            }
        }

        private static string GetConfigString(string key, string defaultValue = default(string))
        {
            OpenConfig();

            if (Config.AppSettings.Settings.AllKeys.Contains(key))
                return Config.AppSettings.Settings[key].Value;
            else
                return defaultValue;
        }

        private static int GetConfigInt(string key, int defaultValue = 0)
        {
            int value;
            if (int.TryParse(GetConfigString(key), out value))
                return value;
            else
                return defaultValue;
        }


        private static string GetEnvString(string key, string defaultValue)
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(key);
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }


        private static int GetEnvInt(string key, int defaultValue)
        {
            int value;
            var sValue = GetEnvString(key, defaultValue.ToString());
            return int.TryParse(sValue, out value) ? value : defaultValue;
        }

        public static class Hosts
        {
            public static string Cassandra
            {
                get
                {
                    return GetEnvString("CASSANDRA_SERVICE_HOST",
                        GetConfigString("Cassandra.Host",
                                        "Cassandra"));
                }
            }

            public static string SQLServer
            {
                get
                {
                    return GetEnvString("SQLSERVER_SERVICE_HOST",
                        GetConfigString("SQLServer.Host",
                                        "SQLServer"));
                }
            }

            public static string Pizzeria
            {
                get
                {
                    return GetEnvString("PIZZERIA_SERVICE_HOST",
                        GetConfigString("Pizzeria.Host",
                                        "Pizzeria"));
                }
            }

            public static string PizzaBaker
            {
                get
                {
                    return GetEnvString("PIZZABAKER_SERVICE_HOST",
                        GetConfigString("PizzaBaker.Host",
                                        "PizzaBaker"));
                }
            }
        }

        public static class Ports
        {
            public static int Pizzeria
            {
                get
                {
                    return GetEnvInt("PIZZERIA_SERVICE_PORT", 
                        GetConfigInt("Pizzeria.Port", 
                                    9090));
                }
            }
            public static int PizzaBaker
            {
                get
                {
                    return GetEnvInt("PIZZABAKER_SERVICE_PORT", 
                        GetConfigInt("PizzaBaker.Port", 
                                    9091));
                }
            }
        }

        public static class ReadinessPorts
        {
            private const int DEFAULT_READINESS_PORT = 9080;

            public static int Pizzeria
            {
                get
                {
                    return GetEnvInt("PIZZERIA_READINESS_PORT", 
                        GetConfigInt("Pizzeria.ReadinessPort", 
                                    DEFAULT_READINESS_PORT));
                }
            }
            public static int PizzaBaker
            {
                get
                {
                    return GetEnvInt("PIZZABAKER_READINESS_PORT", 
                        GetConfigInt("PizzaBaker.ReadinessPort", 
                                    DEFAULT_READINESS_PORT));
                }
            }
        }

        public static DBType DBType
        {
            get
            {
                var sDB = GetEnvString("DATABASE_TYPE", 
                        GetConfigString("Database",  
                                        "Cassandra"));

                if (string.Compare(sDB, "Cassandra", true) == 0)
                    return DBType.Cassandra;
                if (string.Compare(sDB, "SQLServer", true) == 0)
                    return DBType.SQLServer;

                return DBType.Cassandra;  // default
            }
        }


        public static string DBUser
        {
            get
            {
                return "sa";
            }
        }


        public static string DBPassword
        {
            get
            {
                try
                {
                    var sPwd = Environment.GetEnvironmentVariable("sa_password", EnvironmentVariableTarget.Process);
                    return sPwd;
                }
                catch (Exception)
                {
                    Console.WriteLine("WN: missing DBPassword");
                    return string.Empty;
                }
            }
        }

    }
}
