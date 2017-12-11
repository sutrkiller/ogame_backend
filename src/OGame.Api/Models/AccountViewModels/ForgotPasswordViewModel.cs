using System.ComponentModel.DataAnnotations;

namespace OGame.Api.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}