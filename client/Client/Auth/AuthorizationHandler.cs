using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace client.Auth
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        private const string appName = "app";
        private const string identifier = "identifier";
        private IPolicyDataService _policyDataService;

        public AuthorizationHandler(IPolicyDataService policyDataService) 
        {
            _policyDataService = policyDataService;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var sub = context.User.FindFirst("sub")?.Value;
            var scopedId = $"{appName}/{identifier}/{sub}";

            var requirementToSatisfy = context.Requirements.First();
            
            if (requirementToSatisfy is SalaryAuthorizationRequirement) {
                if (_policyDataService.HasPermissions(scopedId, "viewSalary")) {
                    context.Succeed(requirementToSatisfy);
                }
                else {
                    context.Fail();
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
