using Passwork.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    public Guid SafeId { get; set; }
    public List<string> Tags { get; set; }
}
