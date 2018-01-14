using Microsoft.EntityFrameworkCore;
using OGame.Repositories.Entities;
using OGame.Repositories.ModelBuilders;

namespace OGame.Repositories.Contexts
{
    public class ApplicationContext : DbContext
    {
        public virtual DbSet<RunnerEntity> Runners { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {  
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureRunners();
        }
    }
}
