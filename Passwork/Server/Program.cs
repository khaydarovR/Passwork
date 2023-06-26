using Microsoft.AspNetCore.ResponseCompression;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Server.Application.Configure;
using Passwork.Server.Application.Services.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddMy(builder.Configuration);

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

app.Services.CreateScope().ServiceProvider.GetService<ISeedingService>()!.DbInit(false);
app.UseAuthentication();;

app.Run();
