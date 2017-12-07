using System;
using System.ComponentModel.DataAnnotations;

namespace OGame.Api.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "UserId")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}