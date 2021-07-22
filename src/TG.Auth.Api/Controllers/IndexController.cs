using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TG.Auth.Api.App.Monitoring;
using TG.Auth.Api.Config;

namespace TG.Auth.Api.Controllers
{
    [Route(ServiceConst.RoutePrefix)]
    public class IndexController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public IndexController(ILogger logger, IConfiguration configuration, ILogger<IndexController> logger1)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public string Get()
        {
            TgExecutionContext.TrySetTraceIdentifier("trece_id");
            _logger.LogError(new Exception(),"ERROR");
            return "Works " + _configuration.GetValue<string>("Secrets:Greeting");
        }
    }
}