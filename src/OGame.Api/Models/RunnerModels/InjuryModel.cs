using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGame.Api.Models.RunnerModels
{
    public class InjuryModel
    {
        public bool Injured { get; set; }

        public DateTime InjuryDate { get; set; }

        public int EstimatedRecoveryDays { get; set; }
    }
}
