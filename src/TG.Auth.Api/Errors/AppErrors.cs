using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Errors
{
    public static class AppErrors
    {
        public const string UserBannedCode = "user_banned";

        public static readonly ErrorResult NotFound = new ErrorResult("not_found", "Not found");
        public static ErrorResult UserBanned(string message) => new ErrorResult(UserBannedCode, message);
        public static ErrorResult BanActive = new ErrorResult("ban_active", "Ban is active and can not be removed");
    }
}