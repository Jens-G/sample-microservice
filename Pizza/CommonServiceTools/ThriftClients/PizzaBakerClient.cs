using System;
using Pizzeria;
using Thrift.Protocol;
using Thrift.Transport;

namespace ThriftClients
{
    public class PizzaBakerClient : BaseClient<PizzaBaker.PizzaBaker.ISync>
    {

        public PizzaBaker.PizzaBaker.ISync Impl { get; private set; }


        public PizzaBakerClient(string server, int port)
            : base(server, port)
        {
            Impl = ConnectClient();
        }

        protected override PizzaBaker.PizzaBaker.ISync ClientFactory(TProtocol prot)
        {
            return new PizzaBaker.PizzaBaker.Client(prot);
        }

        protected override string MultiplexName()
        {
            return typeof(PizzaBaker.PizzaBaker).Name;
        }

    }
}