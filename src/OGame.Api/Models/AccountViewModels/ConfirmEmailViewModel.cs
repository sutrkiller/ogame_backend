using System;
using System.ComponentModel.DataAnnotations;

namespace OGame.Api.Models.AccountViewModels
{
    public class ConfirmEmailViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}