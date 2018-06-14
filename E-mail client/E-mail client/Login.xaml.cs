using System;
using System.Windows;

namespace E_mail_client
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        

        private void signIn_Click(object sender, RoutedEventArgs e)
        {
            string gmail = gmailData.Text;
            string password = passwordData.Password;

            User.SetUser(gmail, password);

            WindowController.Instance.loginWindow = this;
            WindowController.Instance.loginWindow.Hide();
            WindowController.Instance.allMessagesWindow = new AllMessages();
            WindowController.Instance.allMessagesWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(WindowController.Instance.allMessagesWindow != null)
            {
                WindowController.Instance.allMessagesWindow.Show();
                WindowController.Instance.loginWindow = this;
                WindowController.Instance.loginWindow.Hide();
            }
        }
    }
}
