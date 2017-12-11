using System;
using Microsoft.AspNetCore.Identity;

namespace OGame.Auth.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateTime JoinDate { get; set; }
    }
}
