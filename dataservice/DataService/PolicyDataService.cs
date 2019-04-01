using DataService.Rpc;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static DataService.Rpc.PolicyData;

namespace DataService
{
    class PolicyDataService : PolicyDataBase
    {
        public override Task<PermissionsResponse> GetPermissions(UserRequest request, ServerCallContext context)
        {

            return base.GetPermissions(request, context);
        }

        public override Task<RolesResponse> GetRoles(UserRequest request, ServerCallContext context)
        {
            return base.GetRoles(request, context);
        }

        public override Task<HasPermissionsResponse> HasPermissions(HasPermissionsRequest request, ServerCallContext context)
        {
            return base.HasPermissions(request, context);
        }

        public override Task<HasRoleResponse> HasRole(HasRoleRequest request, ServerCallContext context)
        {
            return base.HasRole(request, context);
        }
    }
}
