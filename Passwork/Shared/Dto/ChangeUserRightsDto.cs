using Passwork.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwork.Shared.Dto;

public class ChangeUserRightsDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    public Guid SafeId { get; set; }

    [Required(ErrorMessage = "Обязательное поле")]
    public RightEnumVm NewRight { get; set; }

    [Required(ErrorMessage = "Обязательное поле")]
    public List<Guid> UserIds { get; set; } = new();
}
