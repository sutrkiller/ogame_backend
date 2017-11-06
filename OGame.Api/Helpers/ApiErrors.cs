using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OGame.Api.Models.NotificationModels;

namespace OGame.Api.Helpers
{
    public static class ApiErrors
    {
        public static IActionResult InvalidModel(ModelStateDictionary modelState)
        {
            var model = new ErrorResponseModel
            {
                Code = ErrorCode.InvalidModel,
                Message = "The validation for some values failed",
                Data = modelState.Keys.SelectMany(x =>
                    modelState[x].Errors.Select(e => new ErrorMessage(x, e.ErrorMessage)))
            };
            return new BadRequestObjectResult(model);
        }
        
        public static IActionResult UnreachableEmail(string email)
        {
            var model = new ErrorResponseModel
            {
                Code = ErrorCode.UnreachableEmail,
                Message = $"An email address {email} could not be reached",
                Data = new List<ErrorMessage> { new ErrorMessage("email", "Unable to send confirmation email to this address") }
            };
            return new BadRequestObjectResult(model);
        }
    }
}