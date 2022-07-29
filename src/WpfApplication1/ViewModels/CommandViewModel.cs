using System;
using System.Windows.Input;

namespace AssetBuilder.ViewModels
{
    public class CommandViewModel : ViewModelBase
    {
        public ICommand Command { get; private set; }

        public CommandViewModel(string displayName, ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            base.DisplayName = displayName;
            Command = command;
        }
    }
}