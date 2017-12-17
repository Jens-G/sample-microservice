using System;
using Pizzeria;
using Thrift.Protocol;
using Thrift.Transport;

namespace ThriftClients
{
    public class PizzeriaCallbackClient : BaseClient<Pizzeria.PizzeriaCallback.ISync>
    {

        public Pizzeria.PizzeriaCallback.ISync Impl { get; private set; }


        public PizzeriaCallbackClient(string server, int port)
            : base(server, port)
        {
            Impl = ConnectClient();
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

        protected override Pizzeria.PizzeriaCallback.ISync ClientFactory(TProtocol prot)
        {
            return new Pizzeria.PizzeriaCallback.Client(prot);
        }

        protected override string MultiplexName()
        {
            return typeof(Pizzeria.PizzeriaCallback).Name;
        }



    }
}