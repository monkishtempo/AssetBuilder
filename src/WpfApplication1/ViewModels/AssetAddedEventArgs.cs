using System;

namespace AssetBuilder.ViewModels
{
    public class AssetAddedEventArgs : EventArgs
    {
        public ListItem NewAsset { get; private set; }

        public AssetAddedEventArgs(ListItem newAsset)
        {
            NewAsset = newAsset;
        }
    }
}