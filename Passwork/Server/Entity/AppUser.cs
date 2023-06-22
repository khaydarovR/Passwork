using Microsoft.AspNetCore.Identity;

namespace Passwork.Server.Entity;

public class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string MasterPassword { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = new();
    
}
