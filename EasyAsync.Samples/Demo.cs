using System;
using System.Collections.Generic;
using System.Text;

namespace EasyAsync.Samples
{
    class Demo
    {
        private static void PrintUsage()
        {
            Console.WriteLine(@"usage:
asyncsample server <port>
asyncsample client <host> <port>
asyncsample sleep
asyncsample monitor
asyncsample abort
asyncsample gui");
        }

        [STAThread]
        public static void Main(string[] args)
        {
            List<string> samples = new List<string> { "server", "client", "sleep", "monitor", "abort", "gui" };

            if (args.Length < 1 || !samples.Contains(args[0].ToLower()))
            {
                PrintUsage();
                return;
            }

            try
            {
                switch (args[0].ToLower())
                {
                    case "sleep":
                        new Sleep().Demo();
                        break;
                    case "monitor":
                        new MonitorSample().Demo();
                        break;
                    case "abort":
                        new Abort().Demo();
                        break;
                    case "gui":
                        new Gui().Demo();
                        break;
                    case "server":
                        {
                            int port = 0;
                            if (args.Length < 2 || !int.TryParse(args[1], out port))
                            {
                                Console.WriteLine("usage: asyncsample server <port>");
                                return;
                            }
                            new Server().Demo(port);
                            break;
                        }
                    case "client":
                        {
                            int port = 0;
                            if (args.Length < 3 || !int.TryParse(args[2], out port))
                            {
                                Console.WriteLine("usage: asyncsample client <host> <port>");
                                return;
                            }
                            new Client().Demo(args[1], port);
                            break;
                        }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
            }
        }
    }
}
