namespace Passwork.Server.Entity;

public class Company: BaseEntity
{
    public string Name { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public AppUser Owner { get; set; } = new();
}
