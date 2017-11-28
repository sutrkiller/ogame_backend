using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using MimeKit.Text;
using OGame.Services.Interfaces;
using Microsoft.Extensions.Options;
using OGame.Configuration.Settings;

namespace OGame.Services
{
    // TODO: consider moving to console application that would read queue with emails to be sent.
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ClientSettings _clientSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings, IOptions<ClientSettings> clientSettings)
        {
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _clientSettings = clientSettings.Value ?? throw new ArgumentNullException(nameof(clientSettings));
        }

        public async Task SendHtmlEmailAsync(string email, string subject, string content)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException(nameof(email));
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = content
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort);
                await client.AuthenticateAsync(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendConfirmationEmailAsync(string email, Guid userId, string token)
        {
            //var query = new QueryHelpers.
            var baseUrl = new Uri(_clientSettings.BaseUrl);
            var url = new Uri(baseUrl, _clientSettings.ConfirmEmailEndpoint).ToString();
            url = QueryHelpers.AddQueryString(url,
                new Dictionary<string, string> {{"userId", userId.ToString()}, {"token", token}});

            var subject = "Security confirmation of OGame account";
            var message = $"<p>Hello!</p> <p>You just created new account with email address {email}.</p><p>If you created this account, confirm it for security reasons by clicking the following address: {url}</p><p>Thank you.<br/>OGame team</p>";

            //TODO: retry or do in a webjob
            await SendHtmlEmailAsync(email, subject, message);
        }
    }
}
