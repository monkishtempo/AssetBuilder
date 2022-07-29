using System;

namespace AssetBuilder.UM.Models
{
    public class UserInfoSessionLoginHistory
    {
        public string DomainLoginAccount { get; set; }
        public DateTime? Current { get; set; }
        public DateTime? Previous { get; set; }
        public DateTime? Former { get; set; }
    }
}