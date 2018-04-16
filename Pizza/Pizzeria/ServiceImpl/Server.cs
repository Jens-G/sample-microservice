using CommonServiceTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Transport;

namespace Pizzeria.ServiceImpl
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

            // create handler/processor for the public service
            Handler handler = new Handler();
            Pizzeria.Processor pizzeria = new Pizzeria.Processor(handler);
            multiplexProcessor.RegisterProcessor(typeof(Pizzeria).Name, pizzeria);

            // create handler/processor for the internal service
            // handler = same 
            PizzeriaCallback.Processor callback = new PizzeriaCallback.Processor(handler);
            multiplexProcessor.RegisterProcessor(typeof(PizzeriaCallback).Name, callback);

            // create handler/processor for the diagnostics service
            // handler = same 
            Diagnostics.Diagnostics.Processor diagnose = new Diagnostics.Diagnostics.Processor(handler);
            multiplexProcessor.RegisterProcessor(typeof(Diagnostics.Diagnostics).Name, diagnose);

            // more processors as needed ...

            // complete internal setup
            Console.Title = Environment.MachineName + "-" + port.ToString();

            ReadinessHttpServer.Start(PizzaConfig.ReadinessPorts.Pizzeria);
            try
            {
                // return the server instance
                //var server = new TThreadPoolServer(multiplexProcessor, serverTransport, transportFactory, protocolFactory);
                var server = new TThreadedServer(multiplexProcessor, serverTransport, transportFactory, protocolFactory);
                //var server = new TSimpleServer(multiplexProcessor, serverTransport, transportFactory, protocolFactory);

                ReadinessHttpServer.Status = Readiness.AliveAndReady;
                server.Serve();
            }
            finally
            {
                ReadinessHttpServer.Stop();
            }
        }
    }


}
