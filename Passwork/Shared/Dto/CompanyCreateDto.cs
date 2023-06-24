using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Passwork.Shared.Dto;

public class CompanyCreateDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Название компани")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Не достаточно символов")]
    public string Name { get; set; } = null!;
}
