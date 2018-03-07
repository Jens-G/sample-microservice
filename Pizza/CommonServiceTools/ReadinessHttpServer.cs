using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CommonServiceTools
{
    public enum Readiness
    {
        Dead,
        AliveButBusy,
        AliveAndReady
    }

    public class ReadinessHttpServer
    {
        private static volatile bool Terminated;

        public static volatile Readiness Status;
        public static bool Alive { get { return Status != Readiness.Dead; } }
        public static bool Ready { get { return Status == Readiness.AliveAndReady; } }

        public static void Stop()
        {
            Terminated = true;
        }

        public static void Start(int nPort = 80)
        {
            Terminated = false;
            Thread thread = new Thread(() =>
            {
                var srv = new TcpListener(IPAddress.Any, nPort);
                srv.Start();
                while (! Terminated)
                {
                    using (var socke = srv.AcceptSocket())
                    {
                        const string RESPONSE = "HTTP/1.0 {0}\r\n"
                                              + "Connection: close\r\n"
                                              + "Content-Length: 0\r\n"
                                              + "\r\n"
                                              ;

                        // GET http://localhost:9080/ready HTTP/1.1
                        string sRequest = string.Empty;
                        while (true)
                        {
                            var bytes = new byte[128];
                            var nCount = socke.Receive(bytes, System.Net.Sockets.SocketFlags.None);
                            sRequest += Encoding.ASCII.GetString(bytes, 0, nCount);

                            // we only want the first line
                            var nPos = sRequest.IndexOfAny(new char[2] { '\r', '\n' });
                            if (nPos >= 0)
                            {
                                sRequest = sRequest.Remove(nPos);
                                break;
                            }
                        }

                        var sStatus = "400 bad request";
                        var pieces = sRequest.Split(' ');
                        if (pieces.Length >= 2)
                        {
                            if (pieces[1].EndsWith("/alive"))
                                sStatus = Alive ? "200 OK" : "500 dead";
                            else if (pieces[1].EndsWith("/ready"))
                                sStatus = Ready ? "200 OK" : "500 busy";
                            else
                                sStatus = "404 not found";
                        }

                        var sResponse = string.Format(RESPONSE, sStatus);
                        var response = Encoding.ASCII.GetBytes(sResponse);
                        socke.Send(response);
                    }
                }
            });

            // Alive and ready for all kinds of weird fun
            thread.Start();
        }
    }

}
