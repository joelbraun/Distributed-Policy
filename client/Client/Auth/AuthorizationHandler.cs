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
            var sub = context.User.FindFirst("sub")?.Value;
            var scopedId = $"{appName}/{identifier}/alice";

            var requirementToSatisfy = context.Requirements.First();
            
            if (requirementToSatisfy is SalaryAuthorizationRequirement) {
                var permissions = _policyDataService.GetPermissions(scopedId);

                var data = new {
                input = new {
                        permissions
                    }
                };

                var result = await _opaClient.PostAsJsonAsync("http://localhost:8181/v1/data/httpapi/authz", data);

                dynamic jsonData = JObject.Parse(await result.Content.ReadAsStringAsync());
                var res = (bool)jsonData.result.allow.Value;

                if (res) {
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
