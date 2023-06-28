using System.ComponentModel.DataAnnotations;

namespace Passwork.Shared.Dto;

public class UserLoginDto
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
    [Display(Name = "Исполльзуеться в качестве ключа для шифрования ваших данных")]
    [DataType(DataType.Password)]
    public string MasterPassword { get; set; }

    [Display(Name = "Запомнить?")]
    public bool RememberMe { get; set; }
}
