using System;

namespace TG.Auth.Api.Models.Response
{
    public class BanResponse
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public string? UserLogin { get; set; }
        
        public DateTime BanTime { get; set; }
        
        public DateTime? BannedTill { get; set; }
        
        public DateTime? UnbannedTime { get; set; }

        public string Reason { get; set; } = default!;
        
        public string? AdminUserLogin { get; set; }
        
        public string? Comment { get; set; }
    }
}