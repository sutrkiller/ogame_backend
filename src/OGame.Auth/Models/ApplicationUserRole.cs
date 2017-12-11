using System;
using Microsoft.AspNetCore.Identity;

namespace OGame.Auth.Models
{
    public class ApplicationUserRole : IdentityRole<Guid>
    {
        public string Description { get; set; }
    }
}
