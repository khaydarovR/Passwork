namespace Passwork.Server.Domain.Entity;

public class Tag : BaseEntity
{
    public string Title { get; set; } = null!;
    public IList<PasswordTags> PasswordTags { get; set; }
}
