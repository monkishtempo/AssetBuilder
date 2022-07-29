using System;

namespace AssetBuilder.UM.Models
{
    public class UserAddedEventArgs : EventArgs
    {
        public AssetUser NewUser { get; set; }

        public UserAddedEventArgs(AssetUser newUser)
        {
            NewUser = newUser;
        }
    }
}