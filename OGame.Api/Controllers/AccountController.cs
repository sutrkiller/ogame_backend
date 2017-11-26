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
using Newtonsoft.Json;
using OGame.Api.Filters;
using OGame.Api.Helpers;
using OGame.Api.Models.AccountViewModels;
using OGame.Services.Interfaces;
using OGame.Auth.Models;

namespace OGame.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
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

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger, IConfiguration configuration, IIdGenerator idGenerator,
            IEmailSender emailSender, IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

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

            _logger.LogInformation($"Creating local account with email '{model.Email}'.");
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning($"Creating of local account with email '{model.Email}' failed.");

                return GetRegisterErrors(result);
            }

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailSender.SendConfirmationEmailAsync(model.Email, user.Id, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to send a confirmation email to address '{model.Email}'");
                return ApiErrors.UnreachableEmail(model.Email);
            }

            _logger.LogInformation($"New local user account with email '{model.Email}' created.");
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("confirmEmail")]
        [ValidateModel]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                _logger.LogError($"Unable to confirm account with user id '{model.UserId}' and token '{model.Token}'");
                return ApiErrors.UnableToConfirmEmail(model.UserId);
            }
            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
            {
                _logger.LogError($"Confirming account with user id '{model.UserId}' and token '{model.Token}' failed. Result: {JsonConvert.SerializeObject(result)}");
                return ApiErrors.UnkownError();
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
                    ModelState.AddModelError(nameof(LoginViewModel),
                        "The combination of this email and password not found.");
                    return BadRequest(ModelState);
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(nameof(LoginViewModel),
                        "The combination of this email and password not found.");
                    return BadRequest(ModelState);
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(nameof(LoginViewModel.Email), "This account is not confirmed.");
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
                return StatusCode((int) HttpStatusCode.InternalServerError, "Error while generating token.");
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
            var callbackUrl = new Uri(baseUrl, Url.Action("ResetPassword", new {userId = user.Id, token}));
            await _emailSender.SendHtmlEmailAsync(user.Email, "Reset password",
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
                // TODO
                // AddRegisterModelErrors(result);
                return BadRequest(ModelState);
            }
            return Ok();
        }

        private IActionResult GetRegisterErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                string property;
                switch (error.Code)
                {
                    case "DuplicateEmail":
                        property = nameof(RegisterViewModel.Email);
                        break;

                    case "DuplicateUserName":
                        property = nameof(RegisterViewModel.UserName);
                        break;

                    default:
                        throw new NotImplementedException(error.Code);
                }
                ModelState.AddModelError(property, error.Description);
            }
            return ApiErrors.InvalidModel(ModelState);
        }
    }
}