using System;
using System.Threading.Tasks;

namespace OGame.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendConfirmationEmailAsync(string email, Guid userId, string token);
    }
}
