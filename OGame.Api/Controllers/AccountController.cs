using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OGame.Api.Filters;
using OGame.Api.Models.AccountViewModels;
using OGame.Services.Interfaces;
using OGame.Auth.Models;

namespace OGame.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IIdGenerator _idGenerator;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger, IConfiguration configuration, IIdGenerator idGenerator, IEmailSender emailSender, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
            _idGenerator = idGenerator;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        //TODO: write to client: Password must be at least 6 chars long and contain one non alpha numeric char and at least one numeric char
        [AllowAnonymous]
        [HttpPost("register")]
        [ValidateModel]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                JoinDate = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("New local user account created.");

                //TODO: change baseUrl to client
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var baseUrl = new Uri(Request.GetUri().GetLeftPart(UriPartial.Authority));
                var callbackUrl = new Uri(baseUrl, Url.Action("ConfirmEmail", new {userId = user.Id, token}));

                await _emailSender.SendEmailAsync(model.Email, "Account confirmation",
                    $@"Please confirm your account by going to this address: {callbackUrl}");

                return Ok(result);
            }
            _logger.LogWarning("Creating local user account failed.");
            AddRegisterModelErrors(result);
            return BadRequest(ModelState);
        }

        [HttpGet]
        [AllowAnonymous]
        [ValidateModel]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                ModelState.AddModelError(nameof(ConfirmEmailViewModel), "Unable to confirm email.");
                return BadRequest(ModelState);
            }
            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to confirm email.");
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("token")]
        [ValidateModel]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(nameof(LoginViewModel), "The combination of this email and password not found.");
                    return BadRequest(ModelState);
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(nameof(LoginViewModel), "The combination of this email and password not found.");
                    return BadRequest(ModelState);
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(nameof(LoginViewModel.Email), "Email not confirmed.");
                    // TODO: resend email confirmation
                    return BadRequest(ModelState);
                }

                var userClaims = await _userManager.GetClaimsAsync(user);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, _idGenerator.GenerateId().ToString()),
                }.Union(userClaims);

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Tokens:Issuer"],
                    audience: _configuration["Tokens:Issuer"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(30),
                    signingCredentials: credentials
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    user = _mapper.Map<UserViewModel>(user)
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while creating token: {e}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error while generating token.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                ModelState.AddModelError(nameof(ForgotPasswordViewModel), "Unable to recover password.");
                return BadRequest(user);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            //TODO: change baseUrl to client and resend to backend...
            var baseUrl = new Uri(Request.GetUri().GetLeftPart(UriPartial.Authority));
            var callbackUrl = new Uri(baseUrl, Url.Action("ResetPassword", new { userId = user.Id, token }));
            await _emailSender.SendEmailAsync(user.Email, "Reset password",
                $"Please reset your password by going to this address: {callbackUrl}.");
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(ResetPasswordViewModel), "Reseting password failed.");
                return BadRequest(ModelState);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                AddRegisterModelErrors(result);
                return BadRequest(ModelState);
            }
            return Ok();
        }

        private void AddRegisterModelErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                string attribute;
                if (error.Code.StartsWith("password", StringComparison.InvariantCultureIgnoreCase))
                {
                    attribute = nameof(RegisterViewModel.Password);
                }
                else
                {
                    throw new NotImplementedException();
                }
                ModelState.AddModelError(attribute, error.Description);
            }
        }
    }
}
