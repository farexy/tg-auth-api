using System;
using TG.Auth.Api.Constants;

namespace TG.Auth.Api.Models.Request
{
    public class BanRequest
    {
        public Guid UserId { get; set; }

        public DateTime? BannedTill { get; set; }

        public BanReason Reason { get; set; }

        public string? Comment { get; set; }
    }
}