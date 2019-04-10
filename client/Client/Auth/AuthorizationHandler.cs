using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace client.Auth
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        private const string appName = "app";
        private const string identifier = "identifier";
        private IPolicyDataService _policyDataService;
        private HttpClient _opaClient;

        public AuthorizationHandler(IPolicyDataService policyDataService, IHttpClientFactory httpClientFactory) 
        {
            _policyDataService = policyDataService;
            _opaClient = httpClientFactory.CreateClient();
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            // pull the client_id claim (the sample JWT is retrieved via client_credentials, so no `sub` claim)
            var client = context.User.FindFirst("client_id")?.Value;
            // I'm scoping the permissions as an example of how you might segment RBAC data per-app
            var scopedId = $"{appName}/{identifier}/{client}";

            var requirementToSatisfy = context.Requirements.First();
            
            if (requirementToSatisfy is SalaryAuthorizationRequirement) {
                // query the policy data (PIP) sidecar for this user's permissions
                var permissions = _policyDataService.GetPermissions(scopedId);

                var data = new {
                input = new {
                        permissions
                    }
                };

                // query the OPA (PDP) sidecar
                // all this OPA policy does is check for the "viewSalary" permission, but that's enough
                // to demonstrate the process
                var result = await _opaClient.PostAsJsonAsync("http://localhost:8181/v1/data/httpapi/authz", data);
                dynamic jsonData = JObject.Parse(await result.Content.ReadAsStringAsync());
                var evaluationResult = (bool)jsonData.result.allow.Value;

                if (evaluationResult) {
                    Console.WriteLine("Authorization context success.");
                    context.Succeed(requirementToSatisfy);
                }
                else {
                    context.Fail();
                }
            }
        }
    }
}
