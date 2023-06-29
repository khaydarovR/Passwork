namespace Passwork.Shared.ViewModels;

public class PasswordDetailVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Pw { get; set; } = null!;
    public string Note { get; set; } = null!;
    public string UseInUtl { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
    public Guid SafeId { get; set; }
    public List<TagVm> Tags { get; set; } = new();
}
