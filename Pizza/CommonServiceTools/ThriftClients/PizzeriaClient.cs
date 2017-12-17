using System;
using Pizzeria;
using Thrift.Protocol;
using Thrift.Transport;

namespace ThriftClients
{
    public class PizzeriaClient : BaseClient<Pizzeria.Pizzeria.ISync>
    {

        public Pizzeria.Pizzeria.ISync Impl { get; private set; }


        public PizzeriaClient(string server, int port, bool silent = false)
            : base(server, port)
        {
            Impl = ConnectClient(silent);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (Impl is IDisposable)
                {
                    ((IDisposable)Impl).Dispose();
                    Impl = null;
                }
            }
        }

        protected override Pizzeria.Pizzeria.ISync ClientFactory(TProtocol prot)
        {
            return new Pizzeria.Pizzeria.Client(prot);
        }

        protected override string MultiplexName()
        {
            return typeof(Pizzeria.Pizzeria).Name;
        }


    }
}