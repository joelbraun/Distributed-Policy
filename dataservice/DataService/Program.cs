using DataService.Rpc;
using System;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Core.Logging;

namespace DataService
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { PolicyData.BindService(new PolicyDataService()), Health.BindService(new HealthService()) },
                Ports = { new ServerPort("localhost", 3000, ServerCredentials.Insecure) }
            };

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            server.Start();

            while (true) {

            }
        }
    }
}
