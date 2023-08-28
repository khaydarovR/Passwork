
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Extensions.Logging;
using Passwork.Server.Application.Configure;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.Application.Services.SignalR;
using System.Text;
using System.Text.Json.Serialization;
Console.OutputEncoding = Encoding.UTF8;
var logger = LogManager.Setup()
    .LoadConfigurationFromFile()
    .GetCurrentClassLogger();

logger.Debug("init main");
Console.WriteLine("test rus русксий буквы Ё");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllersWithViews().AddJsonOptions(x =>
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    builder.Services.AddRazorPages();

    builder.Services.AddMy(builder.Configuration);

    builder.Services.AddStackExchangeRedisCache(options => {
        options.Configuration = "localhost:6379";
        options.InstanceName = "local";
    });


    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    //builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Logging.AddNLog();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.MapHub<ApiHub>("/companyhub");

    app.Services.CreateScope().ServiceProvider.GetService<ISeedingService>()!.DbInit(true);
    app.UseAuthentication(); ;

    app.Run();
}
catch(Exception ex)
{
    logger.Error(ex, "init app errror");
    throw;
}
finally
{
    LogManager.Shutdown();   
}

