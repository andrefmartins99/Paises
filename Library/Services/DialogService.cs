namespace Library.Services
{
    using System.Windows;

    public static class DialogService
    {
        /// <summary>
        /// Show message box
        /// </summary>
        /// <param name="title">title of the message box</param>
        /// <param name="message">message of the message box</param>
        public static void ShowMessageBox(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
