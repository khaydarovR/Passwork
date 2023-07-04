using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Passwork.Shared.Dto;

public class DeleteUsersFromSafeDto
{
    [Required(ErrorMessage = "Обязательное поле")]
    public Guid SafeId { get; set; }
    [Required(ErrorMessage = "Обязательное поле")]
    public List<Guid> UserIds { get; set; } = new();
}
