using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using EasyAsync;

namespace EasyAsync.Samples
{
    class Client
    {
        private IEnumerator<IAsyncCall> SendReceive(string host, int port)
        {
            AsyncCall<IPHostEntry> call = new AsyncCall<IPHostEntry>();

            yield return call
                .WaitOn(cb => Dns.BeginGetHostEntry(host, cb, null)) & Dns.EndGetHostEntry;

            if (!call.Succeeded)
            {
                Console.WriteLine(call.Exception.Message);
                yield break;
            }

            IPAddress addr = call.Result.AddressList[0];

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            AsyncCall call2 = new AsyncCall();
            yield return call2
                .WaitOn(cb => socket.BeginConnect(new IPEndPoint(addr, port), cb, null)) & socket.EndConnect;

            if (!call2.Succeeded)
            {
                Console.WriteLine(call2.Exception.Message);
                yield break;
            }

            byte[] sendBuffer = System.Text.Encoding.ASCII.GetBytes("hello world");
            byte[] receiveBuffer = new byte[1024];
            AsyncCall<int> call3 = new AsyncCall<int>();

            for (int i = 0; i < 128; ++i)
            {
                yield return call3
                    .WaitOn(cb => socket.BeginSend(
                        sendBuffer, 0, sendBuffer.Length, SocketFlags.None, cb, null)) & socket.EndSend;

                int bytesSent = call3.Result;
                Console.WriteLine("{0} sent {1} bytes", socket.LocalEndPoint, bytesSent);

                if (bytesSent > 0)
                {
                    yield return call3
                        .WaitOn(cb => socket.BeginReceive(
                            receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, cb, null)) & socket.EndReceive;

                    Console.WriteLine("{0} read {1} bytes", socket.LocalEndPoint, call3.Result);
                }
            }

            Console.WriteLine("closing socket {0}", socket.LocalEndPoint);

            socket.Close();
        }

        public void Demo(string host, int port)
        {
            for (int i = 0; i < 512; ++i)
            {
                TaskManager.AddTask(SendReceive(host, port));
            }

            TaskManager.RunTasks();
        }
    }
}
