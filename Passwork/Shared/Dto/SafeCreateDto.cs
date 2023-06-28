using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Passwork.Shared.Dto;

public class SafeCreateDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    [Display(Name = "Название сейфа")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Не достаточно символов")]
    public string Title { get; set; } = null!;

    [MaybeNull]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Не указана компания")]
    public Guid CompanyId { get; set; }
}
