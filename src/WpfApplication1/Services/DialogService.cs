using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace AssetBuilder.Services
{
    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(message, caption, buttons, icon);
        }
    }
}