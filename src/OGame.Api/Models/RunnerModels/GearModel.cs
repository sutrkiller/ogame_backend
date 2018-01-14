using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGame.Api.Models.RunnerModels
{
    public class GearModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        //TODO: Image

        public SkillsModel SkillBonus { get; set; }

        public decimal Price { get; set; }

        public decimal Weariness { get; set; } //0 = new - 100 = unusable

        public decimal EstimatedDistance { get; set; }
    }
}
