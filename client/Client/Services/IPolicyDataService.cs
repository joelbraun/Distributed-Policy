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
    public interface IPolicyDataService 
    {
        List<string> GetPermissions(string id);

        List<string> GetRoles(string id);

        bool HasPermissions(string id, params string[] permissions);
        
        bool HasRoles(string id, params string[] roles);
        
    }
}