using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;

namespace PizzaBaker.ServiceImpl
{
    class Server
    {
        static internal void Run(int port)
        {
            // check whether the port is free
            TServerTransport serverTransport = new TServerSocket(port);
            serverTransport.Listen();
            serverTransport.Close();
            serverTransport = new TServerSocket(port);
            
            // one processor to rule them all
            var multiplexProcessor = new TMultiplexedProcessor();

            // create protocol factory, default to "framed binary"
            TProtocolFactory protocolFactory = new TBinaryProtocol.Factory(true, true);
            TTransportFactory transportFactory = new TFramedTransport.Factory();

            // create handler/processor for the baker service
            Handler handler = new Handler();
            PizzaBaker.Processor PizzaBaker = new PizzaBaker.Processor(handler);
            multiplexProcessor.RegisterProcessor(typeof(PizzaBaker).Name, PizzaBaker);
            
            // more processors as needed ...

            // complete internal setup
            Console.Title = Environment.MachineName + "-" + port.ToString();

            // return the server instance
            var server = new TThreadedServer(multiplexProcessor, serverTransport, transportFactory, protocolFactory);
            server.Serve();
        }
    }
}
