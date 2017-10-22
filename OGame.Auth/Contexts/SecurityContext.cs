using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OGame.Auth.Models;

namespace OGame.Auth.Contexts
{
    public class SecurityContext : IdentityDbContext<ApplicationUser, ApplicationUserRole, Guid>
    {
        public SecurityContext(DbContextOptions<SecurityContext> options) : base(options)
        { 
        }
    }
}
