using System;
using Pizzeria;
using Thrift.Protocol;
using Thrift.Transport;

namespace ThriftClients
{
    public class DiagnosticsClient : BaseClient<Diagnostics.Diagnostics.ISync>
    {

        public Diagnostics.Diagnostics.ISync Impl { get; private set; }


        public DiagnosticsClient(string server, int port, bool silent = false)
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

        protected override Diagnostics.Diagnostics.ISync ClientFactory(TProtocol prot)
        {
            return new Diagnostics.Diagnostics.Client(prot);
        }

        protected override string MultiplexName()
        {
            return typeof(Diagnostics.Diagnostics).Name;
        }


    }
}