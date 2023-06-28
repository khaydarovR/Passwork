using System.ComponentModel.DataAnnotations;

namespace Passwork.Shared.ViewModels;

public class PasswordVm
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public List<TagVm> Tags { get; set; } = new();
}
