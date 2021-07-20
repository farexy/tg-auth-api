using System;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TG.Auth.Api.App.Monitoring;
using TG.Auth.Api.Config;

namespace TG.Auth.Api.Controllers
{
    [Route(ServiceConst.RoutePrefix)]
    public class IndexController : ControllerBase
    {
        private readonly ILogger _logger;

        public IndexController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            TgExecutionContext.TrySetTraceIdentifier("trece_id");
            _logger.Error("ERROR", new Exception());
            return "Works";
        }
    }
}