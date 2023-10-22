// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License.

using FeedManager.WebClient.Services;

namespace FeedManager.WebClient;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient<FeedService>()
                        .ConfigureHttpClient(client => {
                            var uri = builder.Configuration["FeedService:BaseUrl"];
                            client.BaseAddress = new Uri(uri);

                            client.DefaultRequestVersion = System.Net.HttpVersion.Version30;
                            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                        })
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            ClientCertificateOptions = ClientCertificateOption.Manual,
                            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                            {
                                return true;
                            }
                        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
