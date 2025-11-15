using System.ComponentModel.DataAnnotations;

public class ChangePasswordModel
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]

    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "New password do not match.")]
    public string RepeatNewPassword { get; set; }
}