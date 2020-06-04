namespace Library.Services
{
    using System.Windows;

    public static class DialogService
    {
        /// <summary>
        /// Show message box
        /// </summary>
        /// <param name="title">Title of the message box</param>
        /// <param name="message">Message of the message box</param>
        public static MessageBoxResult ShowMessageBox(string title, string message)
        {
            if (title == "Error")
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

                return MessageBoxResult.OK;
            }
            else if (title == "Warning")
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

                return MessageBoxResult.OK;
            }
            else if (title == "Maps")
            {
                var answer = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

                return answer;
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

                return MessageBoxResult.OK;
            }
        }
    }
}
