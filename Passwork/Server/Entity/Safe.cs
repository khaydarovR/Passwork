namespace Passwork.Server.Entity;

public class Safe: BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
}
