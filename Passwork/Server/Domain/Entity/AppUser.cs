using Microsoft.AspNetCore.Identity;

namespace Passwork.Server.Domain.Entity;

public class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string MasterPassword { get; set; } = null!;
    public IList<SafeUsers> SafeUsers { get; set; }
    public ICollection<ActivityLog> ChangerHistory { get; set; }
}
