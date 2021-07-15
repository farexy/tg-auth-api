using Microsoft.AspNetCore.Mvc;
using TG.Auth.Api.Config;

namespace TG.Auth.Api.Controllers
{
    [Route(ServiceConst.RoutePrefix)]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Works";
        }
    }
}