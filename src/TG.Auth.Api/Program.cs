using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TG.Auth.Api.Config;
using TG.Core.App.Configuration;

namespace TG.Auth.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureTgKeyVault()
                .ConfigureTgLogging(ServiceConst.ServiceName)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();
                });
    }
}
