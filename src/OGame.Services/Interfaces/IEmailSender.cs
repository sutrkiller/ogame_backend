using System;
using System.Threading.Tasks;

namespace OGame.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendHtmlEmailAsync(string email, string subject, string message);
        Task SendConfirmationEmailAsync(string email, Guid userId, string token);
        Task SendForgotPasswordEmailAsync(string email, Guid userId, string token);
    }
}
