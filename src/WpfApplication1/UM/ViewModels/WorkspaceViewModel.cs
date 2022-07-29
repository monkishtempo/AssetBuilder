using System;
using AssetBuilder.UM.Models;
using AssetBuilder.ViewModels;

namespace AssetBuilder.UM.ViewModels
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        public event EventHandler<StatusEventArgs> StatusChanged;

        public virtual void Refresh()
        {
        }

        protected virtual void OnStatusChange(string message)
        {
            var e = new StatusEventArgs(message);
            var handler = StatusChanged;
            handler?.Invoke(this, e);
        }

        protected WorkspaceViewModel()
        {
        }
    }
}