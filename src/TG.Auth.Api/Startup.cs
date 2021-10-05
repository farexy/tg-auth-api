using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TG.Auth.Api.Config;
using TG.Auth.Api.Config.Options;
using TG.Auth.Api.Db;
using TG.Auth.Api.Extensions;
using TG.Auth.Api.Services;
using TG.Core.App.Configuration;
using TG.Core.App.Configuration.Auth;
using TG.Core.App.Middlewares;
using TG.Core.App.Swagger;
using TG.Core.Db.Postgres;
using TG.Core.ServiceBus.Extensions;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api
{
    public class Startup
    {
        public readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddTgJsonOptions()
                .AddInvalidModelStateHandling(); 
            services.AddHealthChecks();
                //.AddNpgSqlHealthCheck();
            //services.AddKubernetesTgApplicationInsights(Configuration);
            services.AddApiVersioning();

            services.AddPostgresDb<ApplicationDbContext>(Configuration, ServiceConst.ServiceName);

            services.AddCors(cors => cors.AddDefaultPolicy(p =>
            {
                p.AllowAnyHeader();
                p.AllowAnyMethod();
                p.AllowAnyOrigin();
            }));

            services.AddTgAuth(Configuration);
            services.AddMediatR(typeof(Startup));

            services.Configure<AuthJwtTokenOptions>(Configuration.GetSection(nameof(JwtTokenOptions)));
            services.Configure<FacebookOptions>(Configuration.GetSection(nameof(FacebookOptions)));
                
            services.AddAutoMapper<Startup>();
            services.AddTgServices();
            services.AddScoped<ILoginGenerator, LoginGenerator>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddHttpClient<IGoogleApiClient, GoogleApiClient>();
            services.AddHttpClient<IFbApiClient, FbApiClient>();

            services.AddTgSwagger(opt =>
            {
                opt.ServiceName = ServiceConst.ServiceName;
                opt.ProjectName = ServiceConst.ProjectName;
                opt.AppVersion = "1";
            });

            services.AddServiceBus(ServiceConst.ServiceName)
                .Configure(Configuration)
                .ConfigureSbTracing()
                .AddQueueProducer<NewUserAuthorizationMessage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseTgSwagger();

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<TracingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
