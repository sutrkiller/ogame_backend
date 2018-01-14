using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OGame.Api.Filters;
using OGame.Api.Models.RunnerModels;
using OGame.Services.Interfaces;

namespace OGame.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class RunnerController : Controller
    {
        private readonly IRunnerService _runnerService;

        public RunnerController(IRunnerService runnerService)
        {
            _runnerService = runnerService;
        }

        [HttpGet("{runnerId:Guid}")]
        public async Task<IActionResult> GetRunner(Guid runnerId)
        {

            return null;
        }

        [ValidateModel]
        [HttpPost]
        public async Task<IActionResult> CreateRunner([FromBody] CreateRunnerViewModel model)
        {
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var runner = _runnerService.GetByUserId(userId);
            if (runner != null)
            {
                throw new NotImplementedException();
            }

            runner = _runnerService.Create(userId, model.Name);
            if (runner != null)
            {
                throw new NotImplementedException();
            }

            //TODO: map to view model


            return null;
        }
    }
}
