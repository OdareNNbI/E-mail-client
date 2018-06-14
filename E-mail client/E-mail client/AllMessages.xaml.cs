using Pop3Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace E_mail_client
{
    public partial class AllMessages : Window
    {
        event Action<List<MailItem>> OnMessageUpdated;

        public static Button CurrentButton { get; set; }

        List<MailItem> mails = new List<MailItem>();
        List<MailItem> sentMails = new List<MailItem>();
        List<MailItem> receivedMails = new List<MailItem>();
        List<ButtonInfo> messagesButtons = new List<ButtonInfo>();

        List<MailItem> Mails
        {
            get
            {
                return mails;
            }
            set
            {
                mails = value;
                sentMails = mails.Where((x) => x.From != null && x.From.Address.Contains(User.Account.Gmail)).ToList();
                receivedMails = mails.Where((x) => !x.From.Address.Contains(User.Account.Gmail)).ToList();
            }
        }

        public AllMessages()
        {
            InitializeComponent();

            OnMessageUpdated += delegate (List<MailItem> mails)
            {
                ShowAllMessageOnCUI(mails);
            };

            Mails = GetAllMessagesFromLocal();
            OnMessageUpdated(Mails);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            updateMessagesButton.IsEnabled = false;

            await Task.Run(() =>
            {
                var newMails = GetAllMessagesFromServer();
                if (newMails != null)
                {
                    Mails.AddRange(newMails);
                    sentMails = Mails.Where((x) => x.From.Address.Contains(User.Account.Gmail)).ToList();
                    receivedMails = Mails.Where((x) => !x.From.Address.Contains(User.Account.Gmail)).ToList();
                }
            });

            if (OnMessageUpdated != null)
            {
                OnMessageUpdated(Mails);
            }

            updateMessagesButton.IsEnabled = true;
        }

        private void ShowAllMessageOnCUI(List<MailItem> messages)
        {
            //Если список сообщений пуст, то выходим
            if (messages != null)
            {
                //Проходим по всем текущим кнопкам и удаляем их
                foreach (var button in messagesButtons)
                {
                    messagesPanel.Children.Remove(button.button);
                }
                //Очищаем список всех сообщений
                messagesButtons.Clear();

                //Проходим по каждому сообщению
                foreach (var mail in messages)
                {
                    //Создаем кнопку для сообщения
                    Button messageButton = new Button();
                    //На кнопке будет отображется от кого пришло сообщение
                    messageButton.Content = mail.From;
                    //Устанавливаем высоту кнопки
                    messageButton.Height = 50;

                    //Добавляем инфорамацию о кноке сообщения в список
                    messagesButtons.Add(new ButtonInfo(messageButton, mail, messageWebBrowser, fromLabel, subjectLabel, dateLabel, toLabel,attachmentButton));
                    //Добавляем кнопку на экран
                    messagesPanel.Children.Add(messageButton);
                }
            }
        }

        private List<MailItem> GetAllMessagesFromServer()
        {
            List<MailItem> mails = new List<MailItem>();
            try
            {
                using (Pop3Client.Pop3Client client = new Pop3Client.Pop3Client(Commands.GmailSettings.host, Commands.GmailSettings.port))
                {
                    if (client.Login(User.Account))
                    {
                        mails = client.GetAllMessagesFromServer();
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return mails;
        }

        private List<MailItem> GetAllMessagesFromLocal()
        {
            List<MailItem> mails = new List<MailItem>();

            try
            {
                using (Pop3Client.Pop3Client client = new Pop3Client.Pop3Client())
                {
                    mails = client.GetAllMessagesFromLocal();
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return mails == null ? new List<MailItem>() : mails;
        }

        private void inputAccountButton_Click(object sender, RoutedEventArgs e)
        {
            WindowController.Instance.allMessagesWindow.Hide();
            WindowController.Instance.loginWindow.Show();
        }

        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            WindowController.Instance.sendMessageWindow.Show();
            WindowController.Instance.allMessagesWindow.Hide();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            sentMessagesButton.IsEnabled = false;
            receivedMessagesButton.IsEnabled = true;

            ShowAllMessageOnCUI(sentMails);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            sentMessagesButton.IsEnabled = true;
            receivedMessagesButton.IsEnabled = false;

            ShowAllMessageOnCUI(receivedMails);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }

    public class ButtonInfo
    {
        public Button button;
        public MailItem mailItem;
        public WebBrowser webBrowser;
        public Button attachmentButton;

        static MailItem currentMailItem;
        

        public ButtonInfo(Button button, MailItem mailItem, WebBrowser webBrowser, Label fromLabel, Label subjectLabel, Label dateLabel, Label toLabel, Button attachmentButton)
        {
            this.attachmentButton = attachmentButton;
            this.mailItem = mailItem;
            this.button = button;
            this.webBrowser = webBrowser;


            this.button.Click += (sender, del) =>
            {
                this.button.IsEnabled = false;
                if(AllMessages.CurrentButton != null)
                {
                    AllMessages.CurrentButton.IsEnabled = true;
                }

                AllMessages.CurrentButton = this.button;
                attachmentButton.Click -= AttachmentButton_Click;
                currentMailItem = mailItem;
                attachmentButton.Click += AttachmentButton_Click;
                webBrowser.NavigateToString(mailItem.GetData);
                fromLabel.Content = "From: " + mailItem.From;
                subjectLabel.Content = "Subject: " + mailItem.Subject;
                dateLabel.Content = "Date: " + mailItem.Date;
                toLabel.Content = "To: " + mailItem.To;
            };
        }

        private static void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", currentMailItem.GetAttachmentPath());
        }
    }
}
