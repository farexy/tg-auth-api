using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using TG.Auth.Api.App.Monitoring;
using TG.Auth.Api.Config;

namespace TG.Auth.Api.Controllers
{
    [Route(ServiceConst.RoutePrefix)]
    public class IndexController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public IndexController(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public string Get()
        {
            TgExecutionContext.TrySetTraceIdentifier("trece_id");
            _logger.Error("ERROR", new Exception());
            return "Works " + _configuration.GetValue<string>("Secrets:Greeting");
        }
    }
}