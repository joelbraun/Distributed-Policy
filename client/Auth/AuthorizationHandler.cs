using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace client.Auth
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        public AuthorizationHandler() 
        {

        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirementToSatisfy = context.Requirements.First();
            
            if (requirementToSatisfy is SalaryAuthorizationRequirement) {
                context.Fail();
            }
            
            return Task.CompletedTask;
        }
    }
}
