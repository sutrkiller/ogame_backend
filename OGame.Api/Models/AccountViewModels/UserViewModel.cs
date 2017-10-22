using System;

namespace OGame.Api.Models.AccountViewModels
{
    public class UserViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
