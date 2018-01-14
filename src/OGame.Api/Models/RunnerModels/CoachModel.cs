using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGame.Api.Models.RunnerModels
{
    public class CoachModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public decimal PhysicalSpecialization { get; set; }

        public decimal MapSpecialization { get; set; }

        public decimal PsychologySpecialization { get; set; }
    }
}
