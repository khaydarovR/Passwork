using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Passwork.Shared.ViewModels;

public class PasswordDetailVm
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Название")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Логин")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Не достаточно символов")]
    public string Login { get; set; } = null!;

    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Пароль")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Не достаточно символов")]
    public string Pw { get; set; } = null!;
    [Display(Name = "Заметка")]
    public string? Note { get; set; }
    public string? UseInUtl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Guid SafeId { get; set; }
    public List<TagVm> Tags { get; set; } = new();
}
