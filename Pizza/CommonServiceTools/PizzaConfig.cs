using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;

namespace CommonServiceTools
{
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

        private static string GetString(string key, string defaultValue = default(string))
        {
            OpenConfig();

            if (Config.AppSettings.Settings.AllKeys.Contains(key))
                return Config.AppSettings.Settings[key].Value;
            else
                return defaultValue;
        }

        private static int GetInt(string key, int defaultValue = 0)
        {
            int value;
            if (int.TryParse(GetString(key), out value))
                return value;
            else
                return defaultValue;
        }


        public static class Hosts
        {
            public static string Cassandra { get { return GetString("Cassandra.Host", "Cassandra"); } }
            public static string Pizzeria { get { return GetString("Pizzeria.Host", "Pizzeria"); } }
            public static string PizzaBaker { get { return GetString("PizzaBaker.Host", "PizzaBaker"); } }
        }

        public static class Ports
        {
            public static int Pizzeria { get { return GetInt("Pizzeria.Port", 9090); } }  
            public static int PizzaBaker { get { return GetInt("PizzaBaker.Port", 9091); } } 
        }


    }
}
