using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using OGame.Services.Interfaces;
using Microsoft.Extensions.Options;
using OGame.Services.Configuration;
using Microsoft.AspNetCore.WebUtilities;

namespace OGame.Services
{
    // TODO: consider moving to console application that would read queue with emails to be sent.
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IOptions<ClientSettings> _clientSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings, IOptions<ClientSettings> clientSettings)
        {
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
            _clientSettings = clientSettings ?? throw new ArgumentNullException(nameof(clientSettings));
        }

        public async Task SendEmailAsync(string email, string subject, string content)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException(nameof(email));
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.Value.FromEmail));
            message.To.Add(new MailboxAddress(email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = content
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.Value.PrimaryDomain, _emailSettings.Value.PrimaryPort);
                await client.AuthenticateAsync(_emailSettings.Value.UsernameEmail, _emailSettings.Value.UsernamePassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendConfirmationEmailAsync(string email, Guid userId, string token)
        {
            //var query = new QueryHelpers.
            var baseUrl = new Uri(_clientSettings.Value.BaseUrl);
            var url = new Uri(baseUrl, _clientSettings.Value.ConfirmEmailEndpoint).ToString();
            url = QueryHelpers.AddQueryString(url,
                new Dictionary<string, string> {{"userId", userId.ToString()}, {"token", token}});

            var subject = "Security confirmation of OGame account";
            var message = $"<p>Hello!</p> <p>You just created new account with email address {email}.</p><p>If you created this account, confirm it for security reasons by clicking the following address: {url}.</p><p>Thank you.<br/>OGame team</p>";

            //TODO: retry or do in a webjob
            await SendEmailAsync(email, subject, message);
        }
    }
}
