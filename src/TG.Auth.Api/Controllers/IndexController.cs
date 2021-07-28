using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TG.Auth.Api.Config;
using TG.Core.App.Configuration.Monitoring;

namespace TG.Auth.Api.Controllers
{
    [Route(ServiceConst.RoutePrefix)]
    public class IndexController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public IndexController(IConfiguration configuration, ILogger<IndexController> logger)
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