using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    [StringLength(20, MinimumLength = 6, ErrorMessage = "Не достаточно символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Запомнить?")]
    public bool RememberMe { get; set; }
}
