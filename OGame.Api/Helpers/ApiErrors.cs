using System;
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

        public static IActionResult UnableToConfirmEmail(Guid userId)
        {
            var model = new ErrorResponseModel
            {
                Code = ErrorCode.UnableToConfirmEmail,
                Message = "Unable to confirm the account. Please try again later",
                Data = new List<ErrorMessage>()
            };
            return new BadRequestObjectResult(model);
        }

        public static IActionResult IncorrectSignInData()
        {
            var model = new ErrorResponseModel
            {
                Code = ErrorCode.IncorrectSignInData,
                Message = "The combination of this email and password not found.",
                Data = new List<ErrorMessage> { new ErrorMessage("password", "The combination of this email and password not found.")}
            };
            return new BadRequestObjectResult(model);
        }

        public static IActionResult UnkownError()
        {
            var model = new ErrorResponseModel
            {
                Code = ErrorCode.UnkownError,
                Message = "An unknown error occured at the server. Try again later. If this problem persists contact our support.",
                Data = new List<ErrorMessage>()
            };
            return new BadRequestObjectResult(model);
        }
    }
}