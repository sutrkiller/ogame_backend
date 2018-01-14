using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OGame.Api.Models.RunnerModels
{
    public class CreateRunnerViewModel
    {
        [Required]
        [DisplayName("Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        public string Name { get; set; }

        public string Country { get; set; }
    }
}
