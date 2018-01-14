using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGame.Api.Models.RunnerModels
{
    public class SkillsModel
    {
        public decimal Speed { get; set; }
        public decimal Stamina { get; set; }
        public decimal Uphill { get; set; }
        public decimal Downhill { get; set; }

        public decimal MapReading { get; set; }
        public decimal RouteChoice { get; set; }
        public decimal Direction { get; set; }
        public decimal Awareness { get; set; }

        // ?
        public decimal Regeneration { get; set; }
        public decimal Concentration { get; set; }
    }
}
