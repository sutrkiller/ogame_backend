using System;
using System.Collections;
using System.Threading.Tasks;

namespace OGame.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string email, Guid userId, string token);
        Task SendForgotPasswordEmailAsync(string email, Guid userId, string token);
    }
}
