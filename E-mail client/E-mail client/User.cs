using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_mail_client
{
    public class User
    {
        public static User Account { get; set; }

        public string Gmail { get; private set; }
        public string Password { get; private set; }

        private User(string Gmail, string Password)
        {
            this.Gmail = Gmail;
            this.Password = Password;
        }


        public static void SetUser(string Gmail, string Password)
        {
            if (Account == null)
                Account = new User(Gmail, Password);
            else
            {
                Account.Gmail = Gmail;
                Account.Password = Password;
            }
        }

        public override string ToString()
        {
            return $"Gmail {Gmail} Password {Password}";
        }
    }
}
