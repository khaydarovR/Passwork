namespace Passwork.Server.Domain.Entity;

public class Safe : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    public ICollection<Password> Passwords { get; set; }
    public ICollection<SafeUsers> SafeUsers { get; set; }
}
