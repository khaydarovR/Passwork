namespace Passwork.Server.Domain.Entity;

public class Company : BaseEntity
{
    public string Name { get; set; } = null!;
    public Guid AppUserId { get; set; }
    public AppUser Owner { get; set; }
    public ICollection<Safe> Safes { get; set; }
}
