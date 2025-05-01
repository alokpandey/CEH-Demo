using ExceptionHandlingDemo.Middleware;
using ExceptionHandlingDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExceptionHandlingDemo.Tests
{
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(ExceptionHandlingDemo.Controllers.TestController).Assembly);

            // Register the retry policy service
            services.AddSingleton<RetryPolicyService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Add our custom exception handling middleware
            app.UseExceptionHandling();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
