using System.ComponentModel.DataAnnotations;

namespace ATS.API.Models;
public class User: BaseEntity
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; } = true;
    public UserRole Role { get; set; }
}