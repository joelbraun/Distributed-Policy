using DataService.Rpc;
using System;
using Grpc.Core;

namespace DataService
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { PolicyData.BindService(new PolicyDataService()) },
                Ports = { new ServerPort("localhost", 3000, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Greeter server listening on port 3000");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
