// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

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
            services.AddOpenTelemetryWithSharedConfiguration(Program.ServiceName);
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