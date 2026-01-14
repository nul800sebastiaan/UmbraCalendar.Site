using Microsoft.AspNetCore.DataProtection;
using UmbraCalendar.Jobs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Persist Data Protection keys to a stable location for Docker
// This ensures OAuth tokens and antiforgery tokens can be decrypted after container restarts
var keysDirectory = Path.Combine(AppContext.BaseDirectory, "umbraco", "Data", "DataProtection-Keys");
Directory.CreateDirectory(keysDirectory);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

Hangfire.GlobalJobFilters.Filters.Add(new FailureThresholdFilter(app.Services.GetRequiredService<IConfiguration>()));

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();