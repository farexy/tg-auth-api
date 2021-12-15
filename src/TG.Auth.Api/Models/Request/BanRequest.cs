using System;

namespace TG.Auth.Api.Models.Request
{
    public class BanRequest
    {
        public Guid UserId { get; set; }

        public DateTime? BannedTill { get; set; }

        public string Reason { get; set; } = default!;

        public string? Comment { get; set; }
    }
}