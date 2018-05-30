using Pop3Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace E_mail_client
{
    public partial class AllMessages : Window
    {
        event Action<List<MailItem>> OnMessageUpdated;

        List<MailItem> mails = new List<MailItem>();
        List<ButtonInfo> messagesButtons = new List<ButtonInfo>();

        public AllMessages()
        {
            InitializeComponent();

            OnMessageUpdated += delegate (List<MailItem> mails)
            {
                ShowAllMessageOnCUI(mails);
            };
        }



        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            updateMessagesButton.IsEnabled = false;

            await Task.Run(() => mails = (GetAllMessagesFromServer()));

            if (OnMessageUpdated != null)
            {
                OnMessageUpdated(mails);
            }
        }

        private void ShowAllMessageOnCUI(List<MailItem> messages)
        {
            if (messages != null)
            {
                foreach (var button in messagesButtons)
                {
                    messagesPanel.Children.Remove(button.button);
                }

                messagesButtons.Clear();

                foreach (var mail in messages)
                {
                    Button messageButton = new Button();

                    messageButton.Content = mail.From;

                    messageButton.Height = 50;

                    messagesButtons.Add(new ButtonInfo(messageButton, mail, messageWebBrowser, fromLabel, subjectLabel, toLabel));
                    messagesPanel.Children.Add(messageButton);
                }

                updateMessagesButton.IsEnabled = true;
            }
        }

        private List<MailItem> GetAllMessagesFromServer()
        {
            List<MailItem> mails = new List<MailItem>();

            using (Pop3Client.Pop3Client client = new Pop3Client.Pop3Client(Commands.GmailSettings.host, Commands.GmailSettings.port))
            {
                if (client.Login(User.Account))
                {
                    mails = client.GetAllMessagesFromServer();
                }
            }

            return mails;
        }

        private List<MailItem> GetAllMessagesFromLocal()
        {
            List<MailItem> mails = new List<MailItem>();

            using (Pop3Client.Pop3Client client = new Pop3Client.Pop3Client(Commands.GmailSettings.host, Commands.GmailSettings.port))
            {
                mails = client.GetAllMessagesFromLocal();
            }

            return mails;
        }
    }

    public class ButtonInfo
    {
        public Button button;
        public MailItem mailItem;
        public WebBrowser webBrowser;

        public ButtonInfo(Button button, MailItem mailItem, WebBrowser webBrowser, Label fromLabel, Label subjectLabel, Label toLabel)
        {
            this.mailItem = mailItem;
            this.button = button;
            this.webBrowser = webBrowser;

            this.button.Click += (sender, del) =>
            {
                webBrowser.NavigateToString(mailItem.GetData);
                fromLabel.Content = "From: " + mailItem.From;
                subjectLabel.Content = "Subject: " + mailItem.Subject + " \\Date: " + mailItem.Date;
                toLabel.Content = "To: " + mailItem.To;
            };
        }
    }
}
