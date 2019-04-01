using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DataService.Rpc.PolicyData;

namespace client.Services
{
    public class PolicyService : IPolicyService
    {
        private static PolicyDataClient _policyDataClient = new PolicyDataClient(new Grpc.Core.Channel("localhost:3000", ChannelCredentials.Insecure));

        public List<string> GetPermissions(string id) {
            var response = _policyDataClient.GetPermissions(new DataService.Rpc.UserRequest { Id = id });

            return response.Permissions.ToList();
        }

        public List<string> GetRoles(string id) {
            var response = _policyDataClient.GetRoles(new DataService.Rpc.UserRequest { Id = id});

            return response.Roles.ToList();
        }

        public bool HasPermissions(string id, params string[] permissions) {
            var request = new DataService.Rpc.HasPermissionsRequest { Id = id };
            request.Permissions.AddRange(permissions);

            var response = _policyDataClient.HasPermissions(request);

            return response.Result;
        }

        public bool HasRoles(string id, params string[] roles) {
            var request = new DataService.Rpc.HasRolesRequest { Id = id};
            request.Roles.AddRange(roles);

            var response = _policyDataClient.HasRoles(request);

            return response.Result;
        }
    }
}
