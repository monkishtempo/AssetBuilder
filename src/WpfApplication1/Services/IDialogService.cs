using System.Windows;

namespace AssetBuilder.Services
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon);
    }
}