namespace Passwork.Shared.ViewModels;

public class PasswordVm
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<TagVm> Tags { get; set; } = new();
}
