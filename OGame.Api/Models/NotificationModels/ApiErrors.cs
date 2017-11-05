using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OGame.Api.Models.NotificationModels
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

        public static IActionResult DuplicateEmail(string email)
        {
            var model = new ErrorResponseModel
            {
                Code = ErrorCode.DuplicateEmail,
                Message = $"An account with email address {email} already exists, you can login or choose different email address",
                Data = new List<ErrorMessage> {new ErrorMessage("email", "An account with this email address alrady exists")}
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