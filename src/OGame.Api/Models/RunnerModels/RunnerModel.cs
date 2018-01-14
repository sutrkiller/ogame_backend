using System;
using System.Collections.Generic;

namespace OGame.Api.Models.RunnerModels
{
    public class RunnerModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public int Age { get; set; }

        public SkillsModel Skills { get; set; }

        public SkillsModel Bonuses { get; set; }

        public decimal Form { get; set; }

        public decimal Morale { get; set; }

        public decimal Energy { get; set; } // 0.00  - 100.00%

        public InjuryModel CurrentInjury { get; set; }

        public IEnumerable<GearModel> AvailableGear { get; set; } = new List<GearModel>();

        public IEnumerable<GearModel> EquippedGear { get; set; } = new List<GearModel>();

        public IEnumerable<SponsorModel> Sponsors { get; set; } = new List<SponsorModel>();
    }
}
