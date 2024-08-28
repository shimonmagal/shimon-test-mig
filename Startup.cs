using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace MyWebApp
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
            services.AddRazorPages();

            // Add Application Insights
            services.AddApplicationInsightsTelemetry();

            // Add OpenTelemetry
            services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Shimon-CoreTest"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true; // Capture the SQL command text
                        options.EnableConnectionLevelAttributes = true; // Capture connection-level attributes
                    })
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri("http://localhost:4318/v1/traces"); // Replace with your OTLP endpoint
                        opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}