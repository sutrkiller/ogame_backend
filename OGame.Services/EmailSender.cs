using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using OGame.Services.Interfaces;
using OGame.Services.Models;
using Microsoft.Extensions.Options;

namespace OGame.Services
{
    // TODO: consider moving to console application that would read queue with emails to be sent.
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<EmailSettings> _emailSettings;
        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings;
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
    }
}
