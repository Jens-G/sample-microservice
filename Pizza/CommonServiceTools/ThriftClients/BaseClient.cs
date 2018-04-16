using System;
using Thrift.Protocol;
using Thrift.Transport;

namespace ThriftClients
{
    public abstract class BaseClient<I> : IDisposable
        where I : class
    {
        private int Port;
        private string Server;
        protected TTransport Transport;
        protected TProtocol Protocol;


        public BaseClient(string server, int port)
        {
            this.Server = server;
            this.Port = port;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Transport is IDisposable)
                {
                    ((IDisposable)Transport).Dispose();
                    Transport = null;
                }

                if (Protocol is IDisposable)
                {
                    ((IDisposable)Transport).Dispose();
                    Protocol = null;
                }
            }
        }

        protected abstract I ClientFactory(TProtocol prot);
        protected abstract string MultiplexName();

        protected virtual I ConnectClient(bool silent = false)
        {
            const int TIMEOUT = 15 * 1000;

            try
            {
                // try to connect this server using timeout 
                if( ! silent)
                    Console.Write("Testing for server at {0}:{1} ... ", Server, Port);
                using (var test = new TSocket(Server, Port, TIMEOUT))
                    test.Open();

                if (!silent)
                    Console.WriteLine("OK", Server, Port);
                var trans = new TFramedTransport(new TSocket(Server, Port, TIMEOUT));
                var proto = new TBinaryProtocol(trans);
                var mplex = new TMultiplexedProtocol(proto, MultiplexName());

                trans.Open();
                return ClientFactory(mplex);
            }
            catch (Exception e)
            {
                Console.WriteLine("Machine {0} port {1}: {2}", Server, Port, e.Message);
            }

            throw new Exception( string.Format("{0}: Can't reach a server at {1}:{2} ... ", DateTime.UtcNow, Server, Port));
        }

    }
}