using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OGame.Api.Models.NotificationModels
{
    public class ErrorResponseModel
    {
        public ErrorCode Code { get; set; }
        public string Message { get; set; }
        public IEnumerable<ErrorMessage> Data { get; set; } = new List<ErrorMessage>();
    }
}