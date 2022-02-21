using System.Windows;

[System.Runtime.InteropServices.ComVisibleAttribute(true)]
public class ScriptInterface
{
    void errorHandler(string message, string url, string lineNumber)
    {
        MessageBox.Show($"Message: {message}, URL: {url}, Line: {lineNumber}");
    }
}
