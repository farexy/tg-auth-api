using TG.Auth.Api.Entities;
using TG.Auth.Api.Errors;
using TG.Core.App.Extensions;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Extensions
{
    public static class BanExtensions
    {
        public static ErrorResult ToError(this Ban ban) =>
            AppErrors.UserBanned($"Your account was banned for {ban.Reason.ToString().ToSentenceCase()} till {ban.BannedTill}");
    }
}