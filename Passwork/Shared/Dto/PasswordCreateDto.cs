using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Passwork.Shared.Dto;

public class PasswordCreateDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Название")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Не достаточно символов")]
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
    [Display(Name = "Адрес где надо ввести эти данные")]
    public string? UseInUrl { get; set; }
    [Required]
    public Guid SafeId { get; set; }
    [MaybeNull]
    [MaxLength(5)]
    public List<string> Tags { get; set; }
}
