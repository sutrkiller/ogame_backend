namespace OGame.Api.Models.NotificationModels
{
    public enum ErrorCode
    {
        InvalidModel = 401, // model is invalid
        DuplicateEmail = 402, 
        UnreachableEmail = 403, 
    }
}