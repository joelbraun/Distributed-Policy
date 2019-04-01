using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using static Grpc.Health.V1.Health;
using static Grpc.Health.V1.HealthCheckResponse.Types;

namespace DataService
{
    class HealthService : HealthBase
    {
        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HealthCheckResponse {
                Status = ServingStatus.Serving
            });
        }

        public override Task Watch(HealthCheckRequest request, IServerStreamWriter<HealthCheckResponse> responseStream, ServerCallContext context)
        {
            responseStream.WriteAsync(new HealthCheckResponse {
                Status = ServingStatus.Serving
            });
            return Task.CompletedTask;
        }
    }
}
