using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.Application.Services;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;

namespace Passwork.Server.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddMy(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("PostgreDb"));
        });

        services.AddIdentity<AppUser, IdentityRole<Guid>>(options => {
            options.SignIn.RequireConfirmedAccount = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddUserManager<UserManager<AppUser>>()
            .AddDefaultTokenProviders();

        services.AddScoped<ISeedingService, SeedingService>();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 4;
            options.Password.RequiredUniqueChars = 1;
            options.User.RequireUniqueEmail = true;
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.Cookie.Name = "KhaydarovCooks";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
            options.LoginPath = "/Identity/Account/Login";
            options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            options.SlidingExpiration = true;
        });

        services.Configure<SecurityStampValidatorOptions>(o =>
                   o.ValidationInterval = TimeSpan.FromMinutes(50));

        return services;
    }
}
