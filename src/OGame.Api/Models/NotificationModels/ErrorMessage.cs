using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OGame.Api.Models.NotificationModels
{
    public class ErrorMessage
    {
        public ErrorMessage(string name, string message)
        {
            Name = name;
            Message = message;
        }

        public string Name { get; set; }
        public string Message { get; set; }
    }
}
