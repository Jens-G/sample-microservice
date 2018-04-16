using Pizzeria;
using System;
using System.Collections.Generic;
using Thrift;

namespace CommonServiceTools
{
    public class Tools
    {
        public static TException RepackageException(Exception e)
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