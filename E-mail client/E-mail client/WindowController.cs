using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace E_mail_client
{
    class WindowController
    {
        public Login loginWindow;
        public AllMessages allMessagesWindow;
        public SendMessage sendMessageWindow;

        private static WindowController instance;

        public static WindowController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WindowController();
                }
                return instance;
            }
        }


        private WindowController()
        {
            loginWindow = new Login();
            sendMessageWindow = new SendMessage();
        }
    }
}
