namespace Passwork.Server.Domain.Entity;

public class Company : BaseEntity
{
    public string Name { get; set; } = null!;
    public IList<CompanyUsers> CompanyUsers { get; set; }
    public ICollection<Safe> Safes { get; set; }
}
