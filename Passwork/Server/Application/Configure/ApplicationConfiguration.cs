using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.Application.Services;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using System.Security.Claims;

namespace Passwork.Server.Application.Configure;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddMy(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("PostgreDb"));
        });

        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.SignIn.RequireConfirmedEmail = false;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddUserManager<UserManager<AppUser>>()
            .AddDefaultTokenProviders();

        services.AddScoped<ISeedingService, SeedingService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddSignalR(hubOpt =>
        {
            hubOpt.ClientTimeoutInterval = TimeSpan.FromHours(2);
            hubOpt.EnableDetailedErrors = true;
        });
        services.AddSingleton<ApiHub>();
        services.AddSingleton<TgBotService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                JwtOptions.SetKey(config["JWT_KEY"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = JwtOptions.AUDIENCE,
                    ValidateLifetime = true,
                    IssuerSigningKey = JwtOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true
                };

                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Path.ToString().StartsWith("/companyhub/"))
                            context.Token = context.Request.Query["access_token"];
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization((opt) =>
        {
            opt.AddPolicy("Admin", p =>
                p.RequireAssertion(x => x.User.HasClaim(ClaimTypes.Role, RoleEmum.Admin.ToString())
                                        || x.User.HasClaim(ClaimTypes.Role, RoleEmum.User.ToString())));
            opt.AddPolicy("User", p =>
                p.RequireAssertion(x => x.User.HasClaim(ClaimTypes.Role, RoleEmum.User.ToString())));
        });

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

        services.Configure<SecurityStampValidatorOptions>(o =>
                   o.ValidationInterval = TimeSpan.FromDays(1));

        return services;
    }
}
