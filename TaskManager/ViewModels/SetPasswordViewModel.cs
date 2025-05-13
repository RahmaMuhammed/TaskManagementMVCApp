using System.ComponentModel.DataAnnotations;

namespace TaskManager.ViewModels
{
    public class SetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

}
