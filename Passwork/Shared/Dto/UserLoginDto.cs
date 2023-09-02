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
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
