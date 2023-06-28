using System.ComponentModel.DataAnnotations;

namespace Passwork.Shared.Dto;

public class UserRegisterDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    [EmailAddress(ErrorMessage = "Не правильный формат Email")]
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Пароль")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Не достаточно символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Правильно заполните поле")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Повторите пароль")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Не достаточно символов")]
    [DataType(DataType.Password)]
    public string Password2 { get; set; }

    [Required(ErrorMessage = "Правильно заполните поле")]
    [Display(Name = "Исполльзуеться в качестве ключа для шифрования ваших данных")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "Не достаточно символов")]
    [DataType(DataType.Password)]
    public string MasterPassword { get; set; }
}
