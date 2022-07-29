using System;

namespace AssetBuilder.UM.Models
{
    public class StatusEventArgs : EventArgs
    {
        public string Status { get; set; }

        public StatusEventArgs(string status)
        {
            Status = status;
        }
    }
}