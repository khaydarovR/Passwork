namespace Passwork.Server.Domain.Entity;

public class Company : BaseEntity
{
    public string Name { get; set; } = null!;
    public ICollection<Safe> Safes { get; set; }
}
