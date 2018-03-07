using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PizzaBaker.ServiceImpl;
using CommonServiceTools;

namespace PizzaBaker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Up and running.");
            Server.Run();
        }
    }
}
