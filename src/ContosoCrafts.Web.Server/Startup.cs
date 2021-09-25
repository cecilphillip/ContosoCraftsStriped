using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContosoCrafts.Web.Server.Services;
using System;
using Microsoft.AspNetCore.Hosting.Server.Features;
using ContosoCrafts.Web.Server.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;
using Stripe;

namespace ContosoCrafts.Web.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            StripeConfiguration.ApiKey = Configuration["Stripe:SecretKey"];
            services.AddHttpContextAccessor();
            services.AddRazorPages();
            services.AddControllers();
            services.AddSignalR();

            services.AddDistributedMemoryCache();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddSingleton<IProductService, JsonFileProductService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<EventsHub>("/events");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
