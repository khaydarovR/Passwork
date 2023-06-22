using Microsoft.EntityFrameworkCore;
using Passwork.Server.DAL;

namespace Passwork.Server.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationServises(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(config.GetConnectionString("PostgreDb"));
        });

        return services;
    }
}
