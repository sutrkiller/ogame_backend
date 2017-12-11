namespace OGame.Api.Models.NotificationModels
{
    public enum ErrorCode
    {
        InvalidModel = 401, // model is invalid
        DuplicateEmail = 402, 
        UnreachableEmail = 403,
        UnableToConfirmEmail = 404,
        IncorrectSignInData = 405,
        AccountNotFound = 406,
        UnableToRecoverPassword = 407,
        UnableToResetPassword = 408,

        UnkownError = 500
    }
}