using System;

namespace AssetBuilder.UM.Models
{
    public class UserAudit
    {
        public string ProcessName { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
        public string Detail { get; set; }
        public string LastModifiedBy { get; set; }
    }
}