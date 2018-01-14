using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGame.Api.Models
{
    public class SponsorModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal WeaklySubsidy { get; set; }

        public decimal WinBonus { get; set; }

        public decimal TopThreeBonus { get; set; }

        public decimal TopTenBonus { get; set; }

        // TODO: expectations

        public decimal EventLevelModifier { get; set; } //lowers the bonuses based on the level of an event
    }
}
