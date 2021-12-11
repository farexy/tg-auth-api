using System;
using TG.Auth.Api.Constants;

namespace TG.Auth.Api.Models.Response
{
    public class BanResponse
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public DateTime BanTime { get; set; }
        
        public DateTime? BannedTill { get; set; }

        public BanReason Reason { get; set; }
        
        public Guid? AdminUserId { get; set; }
        
        public string? Comment { get; set; }
    }
}