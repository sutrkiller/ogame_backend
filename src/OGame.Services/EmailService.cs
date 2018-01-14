using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using OGame.Services.Interfaces;
using Microsoft.Extensions.Options;
using OGame.Configuration.Contracts.Settings;
using OGame.Repositories.Entities;
using OGame.Repositories.Interfaces;

namespace OGame.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EmailSettings _emailSettings;
        private readonly ClientSettings _clientSettings;

        public EmailService(IOptions<EmailSettings> emailSettings, IOptions<ClientSettings> clientSettings, IEmailRepository emailRepository, IDateTimeProvider dateTimeProvider)
        {
            _emailRepository = emailRepository;
            _dateTimeProvider = dateTimeProvider;
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _clientSettings = clientSettings.Value ?? throw new ArgumentNullException(nameof(clientSettings));
        }

        public async Task SendConfirmationEmailAsync(string email, Guid userId, string token)
        {
            var baseUrl = new Uri(_clientSettings.BaseUrl);
            var url = new Uri(baseUrl, _clientSettings.ConfirmEmailEndpoint).ToString();
            url = QueryHelpers.AddQueryString(url,
                new Dictionary<string, string> {{"userId", userId.ToString()}, {"token", token}});

            var subject = "[OGame] Security confirmation of OGame account";
            var message = $"<p>Hello!</p> <p>You just created new account with email address '{email}'.</p><p>If you created this account, confirm it for security reasons by clicking the following address: {url}</p><p>Thank you.<br/>OGame team</p>";

            await EnqueueEmail(email, subject, message);
        }

        public async Task SendForgotPasswordEmailAsync(string email, Guid userId, string token)
        {
            var baseUrl = new Uri(_clientSettings.BaseUrl);
            var url = new Uri(baseUrl, _clientSettings.ResetPasswordEndpoint).ToString();
            url = QueryHelpers.AddQueryString(url,
                new Dictionary<string, string> {{"userId", userId.ToString()}, {"token", token}});

            var subject = "[OGame] Reset password";
            var message = $"<p>Hello!</p> <p>You just requested resetting your password.</p><p>You can change your password on the following address: {url}</p><p>Thank you.<br/><p>If you haven't requested reseting your password, please ignore this email.</p><br/>OGame team</p>";

            await EnqueueEmail(email, subject, message);
        }

        private async Task EnqueueEmail(string recipient, string subject, string body)
        {
            var email = new EmailEntity
            {
                Recipient = recipient,
                Sender = _emailSettings.FromEmail,
                Subject = subject,
                Content = body,
                Created = await _dateTimeProvider.Now(),
            };

           await _emailRepository.AddAsync(email);
        }
    }
}
