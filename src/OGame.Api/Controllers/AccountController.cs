using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OGame.Api.Filters;
using OGame.Api.Helpers;
using OGame.Api.Models.AccountViewModels;
using OGame.Services.Interfaces;
using OGame.Auth.Models;
using OGame.Configuration.Contracts.Settings;

namespace OGame.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly TokenSettings _tokenSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IIdProvider _idGenerator;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger, IIdProvider idGenerator,
            IEmailService emailService, IMapper mapper, IOptions<TokenSettings> tokenSettings)
        {
            _tokenSettings = tokenSettings.Value ?? throw new ArgumentNullException(nameof(tokenSettings));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ValidateModel]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterViewModel model)
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
                await _emailService.SendConfirmationEmailAsync(model.Email, user.Id, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to send a confirmation email to address '{model.Email}'");
                return ApiErrors.UnreachableEmail(model.Email);
            }

            _logger.LogInformation($"New local user account with email '{model.Email}' created.");
            return Ok(_mapper.Map<UserViewModel>(user));
        }

        [AllowAnonymous]
        [HttpPost("confirmEmail")]
        [ValidateModel]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailViewModel model)
        {
            _logger.LogInformation($"Confirming account with user id '{model.UserId}'.");
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
        [HttpPost("signIn")]
        [ValidateModel]
        public async Task<IActionResult> SignInAsync([FromBody] SignInViewModel model)
        {
            try
            {
                _logger.LogInformation($"Signing user with email '{model.Email}'.");
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogInformation($"User with email '{model.Email}' not found.");
                    return ApiErrors.IncorrectSignInData();
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogInformation($"Incorrect password when signing '{model.Email}'.");
                    return ApiErrors.IncorrectSignInData();
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogInformation($"Signing user with unconfirmed email address '{model.Email}'.");
                }

                var userClaims = await _userManager.GetClaimsAsync(user);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, (await _idGenerator.NewId()).ToString()),
                }.Union(userClaims);

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _tokenSettings.Issuer,
                    audience: _tokenSettings.Issuer,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(_tokenSettings.ExpirationDays),
                    signingCredentials: credentials
                );

                return Ok(new
                {
                    token = new {
                        value = new JwtSecurityTokenHandler().WriteToken(token),
                        expirationDate = token.ValidTo
                    },
                    user = _mapper.Map<UserViewModel>(user)
                });
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while creating token: {e}");
                return ApiErrors.UnkownError();
            }
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetAccountDetailsAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                _logger.LogInformation("User not found.");
                throw new ArgumentException();
            }

            var stringToken = Request.Headers["Authorization"].Select(x => x.Replace("Bearer ", "")).FirstOrDefault();
            var token = new JwtSecurityTokenHandler().ReadJwtToken(stringToken);

            return Ok(new
            {
                token = new
                {
                    value = stringToken,
                    expirationDate = token.ValidTo
                },
                user = _mapper.Map<UserViewModel>(user)
            });
        }

        [HttpPost("signOut")]
        public async Task<IActionResult> SignOutAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                _logger.LogInformation("User not found on SignOut.");
                throw new ArgumentException();
            }
            _logger.LogInformation($"User with email '{email}' signing out.");
            await Task.CompletedTask;
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("forgotPassword")]
        [ValidateModel]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody]ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return ApiErrors.AccountNotFound(model.Email);
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return ApiErrors.UnableToRecoverPassword(model.Email);
            }

            _logger.LogInformation($"Sending recovery link for account '{model.Email}'.");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendForgotPasswordEmailAsync(user.Email, user.Id, token);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("resetPassword")]
        [ValidateModel]
        public async Task<IActionResult> ResetPasswordAsync([FromBody]ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                return ApiErrors.UnableToResetPassword(model.UserId, model.Token);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                return GetResetPasswordErrors(result);
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

        private IActionResult GetResetPasswordErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                // string property;
                switch (error.Code)
                {
                    default:
                        throw new NotImplementedException(error.Code);
                }
                // ModelState.AddModelError(property, error.Description);
            }
            return ApiErrors.InvalidModel(ModelState);
        }
    }
}