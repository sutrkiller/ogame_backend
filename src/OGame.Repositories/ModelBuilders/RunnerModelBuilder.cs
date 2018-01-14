using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OGame.Repositories.Entities;

namespace OGame.Repositories.ModelBuilders
{
    internal static class RunnerModelBuilder
    {
        public static ModelBuilder ConfigureRunners(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RunnerEntity>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            return modelBuilder;
        }
    }
}
