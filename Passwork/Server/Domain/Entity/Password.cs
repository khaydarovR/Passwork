namespace Passwork.Server.Domain.Entity;

public class Password : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Pw { get; set; } = null!;
    public string Note { get; set; } = null!;
    public Guid SafeId { get; set; }
    public Safe Safe { get; set; }
    public IList<PasswordTags> PasswordTags { get; set; }
}
