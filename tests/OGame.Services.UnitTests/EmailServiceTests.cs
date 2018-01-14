using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using OGame.Configuration.Contracts.Settings;
using OGame.Repositories.Entities;
using OGame.Repositories.Interfaces;
using OGame.Services.Interfaces;
using Xunit;

namespace OGame.Services.UnitTests
{
    public class EmailServiceTests
    {
        private readonly EmailSettings _emailSettings;
        private readonly ClientSettings _clientSettings;
        private readonly Mock<IEmailRepository> _emailRepository;
        private readonly EmailService _emailService;

        private readonly DateTime _now = DateTime.UtcNow;

        public EmailServiceTests()
        {
            _clientSettings = new ClientSettings
            {
                BaseUrl = "https://baseUrl.com",
                ConfirmEmailEndpoint = "confirmEmail",
                ResetPasswordEndpoint = "resetPassword"
            };

            _emailSettings = new EmailSettings
            {
                PrimaryDomain = "primary.domain.com",
                PrimaryPort = 587,
                UsernameEmail = "userName@email.com",
                UsernamePassword = "fakePassword",
                FromEmail = "from@email.com",
                ToEmail = "toEmail",
                CcEmail = "ccEmail"
            };

            var emailOptions = Options.Create(_emailSettings);
            var clientOptions = Options.Create(_clientSettings);
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            _emailRepository = new Mock<IEmailRepository>();
            _emailRepository.Setup(x => x.AddAsync(It.IsAny<EmailEntity>()))
                .Returns<EmailEntity>(Task.FromResult);
            dateTimeProvider.Setup(x => x.Now())
                .ReturnsAsync(_now);
            _emailService = new EmailService(emailOptions, clientOptions, _emailRepository.Object, dateTimeProvider.Object);
        }

        [Fact]
        public async Task SendConfirmationEmailAsync_EnqueuesEmail()
        {
            var userId = Guid.NewGuid();
            var token = "token";
            var expectedUrl = $"{_clientSettings.BaseUrl.ToLower()}/{_clientSettings.ConfirmEmailEndpoint}?userId={userId.ToString()}&token={token}";
            var recipient = "test@email.com";

            var expected = new EmailEntity
            {
                Recipient = recipient,
                Sender = _emailSettings.FromEmail,
                Subject = "[OGame] Security confirmation of OGame account",
                Content = $"<p>Hello!</p> <p>You just created new account with email address '{recipient}'.</p><p>If you created this account, confirm it for security reasons by clicking the following address: {expectedUrl}</p><p>Thank you.<br/>OGame team</p>",
                Created = _now
            };

            await _emailService.SendConfirmationEmailAsync(recipient, userId, token);

            _emailRepository.Verify(x => x.AddAsync(It.Is<EmailEntity>(e =>
                JToken.DeepEquals(JToken.FromObject(e), JToken.FromObject(expected)))));
        }

        [Fact]
        public async Task SendForgotPasswordEmailAsync_EnqueuesEmail()
        {
            var userId = Guid.NewGuid();
            var token = "token";
            var expectedUrl = $"{_clientSettings.BaseUrl.ToLower()}/{_clientSettings.ResetPasswordEndpoint}?userId={userId.ToString()}&token={token}";
            var recipient = "test@email.com";

            var expected = new EmailEntity
            {
                Recipient = recipient,
                Sender = _emailSettings.FromEmail,
                Subject = "[OGame] Reset password",
                Content = $"<p>Hello!</p> <p>You just requested resetting your password.</p><p>You can change your password on the following address: {expectedUrl}</p><p>Thank you.<br/><p>If you haven't requested reseting your password, please ignore this email.</p><br/>OGame team</p>",
                Created = _now
            };

            await _emailService.SendForgotPasswordEmailAsync(recipient, userId, token);

            _emailRepository.Verify(x => x.AddAsync(It.Is<EmailEntity>(e =>
                JToken.DeepEquals(JToken.FromObject(e), JToken.FromObject(expected)))));
        }
    }
}
