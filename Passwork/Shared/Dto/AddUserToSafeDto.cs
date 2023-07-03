using Passwork.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwork.Shared.Dto;

public class AddUserToSafeDto
{
    [Required(ErrorMessage = "Email обязательно")]
    public string UserEmail { get; set; }
    [Required(ErrorMessage = "safeId обязательно")]
    public Guid SafeId { get; set; }
    public RightEnumVm Right { get; set; }
}
