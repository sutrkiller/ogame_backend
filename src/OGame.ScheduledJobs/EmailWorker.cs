using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using OGame.Configuration.Contracts.Settings;
using OGame.Repositories.Entities;
using OGame.Repositories.Interfaces;
using OGame.Services.Interfaces;

namespace OGame.ScheduledJobs
{
    public class EmailWorker : ITaskWorker
    {
        private const int MaxRetries = 5;
        private readonly IEmailRepository _emailRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<EmailWorker> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailWorker(IEmailRepository emailRepository, IDateTimeProvider dateTimeProvider, ILogger<EmailWorker> logger, IOptions<EmailSettings> emailSettings)
        {
            _emailRepository = emailRepository;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var emails = await _emailRepository.GetAllAsync();
            var notDelivered = emails.Where(x => !x.IsDelivered);
            //TODO: hook on new emails
            foreach (var email in notDelivered)
            {
                var now = await _dateTimeProvider.Now();
                try
                {
                    _logger.LogInformation("Sending email...", email);
                    await SendEmail(email);
                    await _emailRepository.RemoveAsync(email.Id);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Sending email failed.", email);
                    email.FailedTimes++;
                    email.LastSent = now;
                    if (email.FailedTimes >= MaxRetries)
                    {
                        await _emailRepository.RemoveAsync(email.Id);
                    }
                    else
                    {
                        await _emailRepository.UpdateAsync(email);
                    }
                }
            }
        }

        private async Task SendEmail(EmailEntity email)
        {
            if (!new EmailAddressAttribute().IsValid(email.Recipient))
            {
                throw new ArgumentException(nameof(email.Recipient));
            }

            if (!new EmailAddressAttribute().IsValid(email.Sender))
            {
                throw new ArgumentException(nameof(email.Sender));
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(email.Sender));
            message.To.Add(new MailboxAddress(email.Recipient));
            message.Subject = email.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = email.Content
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort);
                await client.AuthenticateAsync(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
