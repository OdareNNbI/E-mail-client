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

            AllMessages allMessages = new AllMessages();
            allMessages.Show();
            this.Hide();
        }
        
    }
}
