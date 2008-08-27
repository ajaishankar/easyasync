using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using EasyAsync;

namespace EasyAsync.Samples
{
    class Server
    {
        private static IEnumerator<IAsyncCall> Listen(int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            Socket sock = new Socket(endPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(endPoint);
            sock.Listen(2058);

            Console.WriteLine("waiting for connections");

            AsyncCall<Socket> call = new AsyncCall<Socket>();
            while (true)
            {
                yield return call.WaitOn(cb => sock.BeginAccept(cb, null)) & sock.EndAccept;

                Socket client = call.Result;

                Console.WriteLine("accepted client {0}", client.RemoteEndPoint);

                TaskManager.AddTask(Echo(client));
            }
        }

        private static IEnumerator<IAsyncCall> Echo(Socket client)
        {
            byte[] buffer = new byte[1024];
            AsyncCall<int> call = new AsyncCall<int>();

            while (true)
            {
                yield return call
                    .WaitOn(cb => client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, cb, null))
                    & client.EndReceive;

                int bytes = call.Result;
                if (bytes > 0)
                {
                    Console.WriteLine("read {0} bytes from {1}", bytes, client.RemoteEndPoint);

                    yield return call
                        .WaitOn(cb => client.BeginSend(buffer, 0, bytes, SocketFlags.None, cb, null))
                        & client.EndReceive;

                    Console.WriteLine("sent {0} bytes to {1}", bytes, client.RemoteEndPoint);
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("closing client socket {0}", client.RemoteEndPoint);
            
            client.Close();
        }

        public void Demo(int port)
        {
            TaskManager.AddTask(Listen(port));
            TaskManager.RunTasks();
        }
    }
}
