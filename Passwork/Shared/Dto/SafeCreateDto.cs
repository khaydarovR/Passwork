using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Passwork.Shared.Dto;

public class SafeCreateDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Название сейфа")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Не достаточно символов")]
    public string Title { get; set; } = null!;

    [MaybeNull]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Обязательное поле")]
    public Guid CompanyId { get; set; }
}
