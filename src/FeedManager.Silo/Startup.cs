// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace FeedManager.Silo
{
    public sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<AggregatedFeedService>();
            services.AddControllers();
            services.AddHealthChecks();
            services.AddOpenTelemetry()
                          .ConfigureResource(resource => resource.AddService(Program.ServiceName))
                          .WithTracing(tracing => tracing
                              .AddAspNetCoreInstrumentation()
                              //.AddHttpClientInstrumentation()
                              .AddConsoleExporter())
                          .WithMetrics(metrics => metrics
                              .AddAspNetCoreInstrumentation()
                              .AddConsoleExporter());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();

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

            app.UseHealthChecks("/healthCheck");

            app.Map("/dashboard", x => x.UseOrleansDashboard());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}