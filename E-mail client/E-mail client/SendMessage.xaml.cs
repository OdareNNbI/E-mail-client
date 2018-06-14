using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SMTP;

namespace E_mail_client
{
    /// <summary>
    /// Логика взаимодействия для SensMessage.xaml
    /// </summary>
    public partial class SendMessage : Window
    {
        public SendMessage()
        {
            InitializeComponent();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SmtpConnection smtpConnection = new SmtpConnection())
                {
                    smtpConnection.Connect("smtp.gmail.com", 587);
                    smtpConnection.ExtendedHello("Hello");
                    smtpConnection.StartTls("smtp.gmail.com");
                    smtpConnection.ExtendedHello("Hello");


                    smtpConnection.AuthPlain(User.Account.Gmail, User.Account.Password);

                    smtpConnection.Mail(User.Account.Gmail);
                    smtpConnection.Recipient(toTextBox.Text);
                    smtpConnection.Data(EmailFormatter.GetText(User.Account.Gmail, subjectTextBox.Text, toTextBox.Text, null, messageText.Text));

                    MessageBox.Show("OK");
                }
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                WindowController.Instance.sendMessageWindow.Hide();
                WindowController.Instance.allMessagesWindow.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WindowController.Instance.allMessagesWindow.Show();
            WindowController.Instance.sendMessageWindow.Hide();
        }
    }
}
