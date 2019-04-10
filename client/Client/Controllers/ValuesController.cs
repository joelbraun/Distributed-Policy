using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IPolicyDataService _policyService;

        public ValuesController(IPolicyDataService policyService) {
            _policyService = policyService;
        }

        // GET api/values
        [HttpGet]
        [Authorize(Policy = "Salary")]
        public ActionResult<IEnumerable<string>> Get()
        {
            // another example of using the data service
            var roles = _policyService.GetRoles("app/identifier/client");

            return new JsonResult(new {
                status = "Authorized",
                roles
            });
        }
    }
}
