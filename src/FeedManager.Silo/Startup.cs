// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.Silo.Services.HealthCheck;
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
            services.AddHealthChecks()
                    .AddCheck<SiloHealthCheck>("siloHealthCheck");

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                services.AddOpenTelemetryWithSharedConfiguration(Program.ServiceName, configuration, MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
            }
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

            app.UseApplicationLifetimeLinkedCancellationToken();

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