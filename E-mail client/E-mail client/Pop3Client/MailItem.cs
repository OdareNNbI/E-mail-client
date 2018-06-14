using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pop3Client
{
    public class MailItem : MailItemBase
    {
        private MailAddress from = null;
        private MailAddress to = null;
        private string dateTime;
        // Тема письма
        public string Subject
        {
            get
            {
                if (this.Headers.ContainsKey("Subject")) return this.Headers["Subject"].ToString();
                return "\\Without subject\\";
            }
        }
        // Отправитель письма
        public MailAddress From
        {
            get
            {
                if (from == null)
                {
                    if (!this.Headers.ContainsKey("From")) return null;
                    from = ParseMail(this.Headers["From"].ToString());
                }
                return from;
            }
        }

        public MailAddress To
        {
            get
            {
                if (to == null)
                {
                    if (!this.Headers.ContainsKey("Delivered-To") && this.Headers.ContainsKey("To"))
                        to = ParseMail(this.Headers["To"].ToString());
                    else if (!this.Headers.ContainsKey("Delivered-To"))
                        return null;
                    else 
                        to = ParseMail(this.Headers["Delivered-To"].ToString());
                }
                return to;
            }
        }

        public string Date
        {
            get
            {
                if (dateTime != null) return dateTime;
                if(this.Headers.ContainsKey("Date"))
                {
                    return (string)Headers["Date"];
                }
                return ":(";
            }
        }

        public string GetData
        {
            get
            {
                if (htmlText != "This message don't have text part")
                    return "<!DOCTYPE HTML><html><head><meta http-equiv = 'Content-Type' content = 'text/html;charset=UTF-8'></head><body>" + htmlText + "</body></html>";
                else return text;
            }
        }

        public MailItem(string source,string attachmentPath = "") : base(source, attachmentPath)
        {
           // TryGetHtmlBody(Data, ref htmlText);
        }

        public static string HTMLTEXT = "This message don't have text part";
        public static string TEXT = "This message don't have text part";

        public string htmlText = "This message don't have text part";
        public string text = "This message don't have text part";

        // Функция парсит адрес электронной почты и возвращает объект типа MailAddress
        private MailAddress ParseMail(string source)
        {
            if (String.IsNullOrEmpty(source)) return null;
            Regex myReg = new Regex(@"(?<name>[^\<]*?)[\<]{0,1}(?<email>[A-Za-zА-Яа-яЁё0-9_\-\.]+@[A-Za-zА-Яа-яЁё0-9_\-\.]+)[\>]{0,1}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match m = myReg.Match(source);
            if (m == null || String.IsNullOrEmpty(m.Value)) return null;
            string email = m.Groups["email"].Value.Trim();
            if (String.IsNullOrEmpty(email)) return null;
            string name = m.Groups["name"].Value.Trim("\" ".ToCharArray());
            return new MailAddress(email, name);
        }

        public void SetAttachmentPath(string path)
        {
            this.attachmentPath = path;
        }

        public string GetAttachmentPath()
        {
            return attachmentPath;
        }
    }
}
