using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Domain.Entity;

namespace Passwork.Server.DAL;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid,
                                            IdentityUserClaim<Guid>,
                                            IdentityUserRole<Guid>,
                                            IdentityUserLogin<Guid>,
                                            IdentityRoleClaim<Guid>,
                                            IdentityUserToken<Guid>>
{
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Safe> Safes { get; set; }
    public DbSet<SafeUsers> SafeUsers { get; set; }
    public DbSet<Password> Passwords { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PasswordTags> PasswordTags { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var appUserBuilder = modelBuilder.Entity<AppUser>();
        appUserBuilder.HasKey(c => c.Id);
        appUserBuilder.HasIndex(c => c.Id).IsUnique();
        appUserBuilder.Property(c => c.MasterPassword).IsRequired();

        var companyBuilder = modelBuilder.Entity<Company>();
        companyBuilder.HasKey(c => c.Id);
        companyBuilder.HasIndex(c => c.Id).IsUnique();
        companyBuilder.Property(c => c.Name).IsRequired();
        companyBuilder.HasOne(c => c.Owner)
            .WithMany(u => u.Companies)
            .HasForeignKey(c => c.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SafeUsers>()
            .HasKey(sc => new { sc.SafeId, sc.AppUserId });
        modelBuilder.Entity<SafeUsers>()
            .Property(c => c.Right).IsRequired();

        var safeBuilder = modelBuilder.Entity<Safe>();
        safeBuilder.HasKey(c => c.Id);
        safeBuilder.HasIndex(c => c.Id).IsUnique();
        safeBuilder.Property(c => c.Title).IsRequired();
        safeBuilder.Property(c => c.Description).IsRequired(false);
        safeBuilder.Ignore(c => c.Company);
        safeBuilder.HasOne(c => c.Company)
            .WithMany(s => s.Safes)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        var passwordBuilder = modelBuilder.Entity<Password>();
        passwordBuilder.HasKey(c => c.Id);
        passwordBuilder.HasIndex(c => c.Id).IsUnique();
        passwordBuilder.Property(c => c.Title).IsRequired();
        passwordBuilder.Property(c => c.Login).IsRequired();
        passwordBuilder.Property(c => c.Pw).IsRequired();
        passwordBuilder.Property(c => c.UseInUtl).IsRequired(false);
        passwordBuilder.Property(c => c.Note).IsRequired(false);
        passwordBuilder.Property(c => c.IsDeleted).HasDefaultValue(false);
        passwordBuilder.HasOne(c => c.Safe)
            .WithMany(s => s.Passwords)
            .HasForeignKey(c => c.SafeId)
            .OnDelete(DeleteBehavior.Cascade);

        var tagBuilder = modelBuilder.Entity<Tag>();
        tagBuilder.HasKey(c => c.Id);
        tagBuilder.HasIndex(c => c.Id).IsUnique();
        tagBuilder.Property(c => c.Title).IsRequired();

        modelBuilder.Entity<PasswordTags>()
            .HasKey(sc => new { sc.PasswordId, sc.TagId });

        var changesBuilder = modelBuilder.Entity<ActivityLog>();
        changesBuilder.HasKey(c => c.Id);
        changesBuilder.HasIndex(c => c.Id).IsUnique();
        changesBuilder.Property(c => c.Title).IsRequired();
        changesBuilder.Property(c => c.At).HasDefaultValue(DateTime.UtcNow);
        changesBuilder.HasOne(c => c.Password)
            .WithMany(s => s.ActivityLog)
            .HasForeignKey(c => c.PasswordId)
            .OnDelete(DeleteBehavior.Cascade);
        changesBuilder.HasOne(c => c.AppUser)
            .WithMany(s => s.ChangerHistory)
            .HasForeignKey(c => c.AppUsreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
