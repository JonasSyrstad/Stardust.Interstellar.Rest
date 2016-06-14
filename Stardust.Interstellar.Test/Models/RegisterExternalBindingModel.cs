using System.ComponentModel.DataAnnotations;

namespace Stardust.Interstellar.Test.Models
{
    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}