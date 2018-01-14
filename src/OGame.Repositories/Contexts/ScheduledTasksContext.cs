using Microsoft.EntityFrameworkCore;
using OGame.Repositories.Entities;

namespace OGame.Repositories.Contexts
{
    public class ScheduledTasksContext : DbContext
    {
        public virtual DbSet<EmailEntity> Emails { get; set; }

        public ScheduledTasksContext(DbContextOptions<ScheduledTasksContext> options) : base(options)
        {  
        }
    }
}
