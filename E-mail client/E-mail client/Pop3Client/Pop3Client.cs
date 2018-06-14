using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Commands;
using E_mail_client;

namespace Pop3Client
{
    public class Pop3Client : IDisposable
    {
        private TcpClient client;
        private SslStream sslStream;
        private string host;
        private int? port = null;

        public bool IsConnectionActive
        {
            get
            {
                return RequestAndAnswer(Commands.RequestCommands.CHECK_CONNECTION).Contains(Commands.ResponseCommands.OK);
            }
        }

        public Pop3Client()
        {

        }
        
        public Pop3Client(string host, int port)
        {
            this.host = host;
            this.port = port;
            client = new TcpClient();

            if (!Connect(host, port))
            {
                MessageBox.Show("NO CONNECTION");
            }
        }

        public bool Connect(string host, int port)
        {
            this.host = host;
            this.port = port;

            client.Connect(host, port);
            sslStream = new SslStream(client.GetStream());
            sslStream.AuthenticateAsClient(host);
            sslStream.ReadTimeout = 1000000000;
            sslStream.WriteTimeout = 1000000000;
            string response = StreamHelper.ReadLineAsString(sslStream);
            if (response.IndexOf(ResponseCommands.OK) == 0)
                return true;

            return false;
        }

        public bool Login(User user)
        {
            string resp = RequestAndAnswer(RequestCommands.USER + user.Gmail);
            if (!IsAnswerOK(resp))
            {
                MessageBox.Show(resp);
                return false;
            }

            resp = RequestAndAnswer(RequestCommands.PASSWORD + user.Password);
            if (!IsAnswerOK(resp))
            {
                MessageBox.Show(resp);
                Directory.CreateDirectory("C:\\Messages\\" + User.Account.Gmail);
                return false;
            }

            return true;
        }

        public void Exit()
        {
            RequestAndAnswer(RequestCommands.QUIT);
        }

        public MailItem CreateMailItem(string messageInfo,string attachmentPath, string newAttachmentPath = "")
        {
            MailItem mail = new MailItem(messageInfo, newAttachmentPath);

            mail.SetAttachmentPath(attachmentPath);
            mail.htmlText = MailItem.HTMLTEXT;
            mail.text = MailItem.TEXT;

            MailItem.HTMLTEXT = "This message don't have text part";
            MailItem.TEXT = "This message don't have text part";

            return mail;
        }

        public MailItem GetMessageFromServer(int number, out bool mailSuccess)
        {
            mailSuccess = false;
            MailItem mail = null;
            string messageUIDL = RequestAndAnswer(Commands.RequestCommands.UIDL_OF_MESSAGE + number.ToString());
            if (IsAnswerOK(messageUIDL))
            {

                Directory.CreateDirectory("C:\\Messages\\" + User.Account.Gmail);
                messageUIDL = messageUIDL.Substring(messageUIDL.IndexOf(" ") + 1);
                messageUIDL = messageUIDL.Substring(messageUIDL.IndexOf(" ") + 1);
                string directoryName = "C:\\Messages\\" + User.Account.Gmail + "\\" + messageUIDL;
                Directory.CreateDirectory(directoryName);
                Directory.CreateDirectory(directoryName + "\\Attachment\\");
                
                string success = RequestAndAnswer(RequestCommands.RETURN_MESSAGE + number.ToString());
                
                if (!File.Exists((directoryName + "\\" + messageUIDL + ".txt")))
                {
                    StreamWriter writer = new StreamWriter("C:\\Messages\\" + User.Account.Gmail + "\\AllMessageUIDL.txt", true);
                    writer.WriteLine(messageUIDL);
                    writer.Close();

                    string answer = null;
                    if (IsAnswerOK(success))
                    {
                        answer = ReadStreamAsString(sslStream);

                        mailSuccess = answer == null;

                        if(mailSuccess)
                         mail = CreateMailItem(answer, directoryName + "\\Attachment\\", directoryName + "\\Attachment\\");
                    }
                    else
                    {
                        return null;
                    }

                    if (answer != null)
                    {
                        FileStream messageFile = new FileStream((directoryName + "\\" + messageUIDL + ".txt"), FileMode.CreateNew);
                        StreamWriter readerStream = new StreamWriter(messageFile);
                        readerStream.WriteLine(answer);
                        readerStream.Close();
                        messageFile.Close();
                    }
                }
            }

            return mail;
        }

        public List<MailItem> GetAllMessagesFromLocal()
        {
            if (!Directory.Exists("C:\\Messages\\" + User.Account.Gmail) || !File.Exists("C:\\Messages\\" + User.Account.Gmail + "\\AllMessageUIDL.txt"))
            {
                MessageBox.Show("NOT FOUND MESSAGES FROM GMAIL: " + User.Account.Gmail + " AND THIS PASSWORD: " + User.Account.Password);
                return null;
            }
            StreamReader reader = new StreamReader("C:\\Messages\\" + User.Account.Gmail + "\\AllMessageUIDL.txt");

            List<MailItem> messages = new List<MailItem>();
            while (!reader.EndOfStream)
            {
                string messageUIDL = reader.ReadLine();
                string directoryName = "C:\\Messages\\" + User.Account.Gmail + "\\" + messageUIDL;
                if (File.Exists((directoryName + "\\" + messageUIDL + ".txt")))
                {
                    StreamReader messageReader = new StreamReader(directoryName + "\\" + messageUIDL + ".txt");
                    string messageInfo = messageReader.ReadToEnd();

                    MailItem mail = CreateMailItem(messageInfo,directoryName  + "\\Attachment\\");
                    messages.Add(mail);

                    messageReader.Close();
                }
                else
                {
                    MessageBox.Show("NOT FOUND MESSAGE");
                }
            }

            reader.Close();

            return messages;
        }

        public List<MailItem> GetAllMessagesFromServer()
        {
            MailItem item = null;
            List<MailItem> mails = new List<MailItem>();
            int count = 1;

            bool mailSuccess;
            while (true) 
            {
                item = GetMessageFromServer(count, out mailSuccess);

                if (mailSuccess)
                {
                    mails.Add(item);
                }
                else if(item == null)
                {
                    break;
                }
                count++;
            }

            return mails;
        }

        private string RequestAndAnswer(string request)
        {
            if (Request(request))
            {
                string response = StreamHelper.ReadLineAsString(sslStream);
                return response;
            }
            else
            {
                return null;
            }
        }

        private bool IsAnswerOK(string answer)
        {
            if (answer == null)
                return false;

            if (answer.IndexOf(Commands.ResponseCommands.OK) != -1)
                return true;

            return false;
        }

        private bool Request(string message)
        {
            if (sslStream != null && client != null)
            {
                message += "\r\n";
                byte[] buffer = (new ASCIIEncoding()).GetBytes(message);

                sslStream.Write(buffer, 0, buffer.Length);
                return true;
            }
            return false;
        }

        private string ReadStreamAsString(Stream stream)
        {
            if (stream == null) throw new Exception("Stream is null");
            
            using (MemoryStream byteArrayBuilder = new MemoryStream())
            {
                bool first = true;
                byte[] lineRead;
                
                while (!IsLastLineInMultiLineResponse(lineRead = StreamHelper.ReadLineAsBytes(stream))) 
                {
                    
                    if (!first)
                    {
                        byte[] crlfPair = Encoding.ASCII.GetBytes("\r\n");
                        byteArrayBuilder.Write(crlfPair, 0, crlfPair.Length);
                    }
                    else
                    {
                        first = false;
                    }
                    
                    if (lineRead.Length > 0 && lineRead[0] == '.')
                    {
                        byteArrayBuilder.Write(lineRead, 1, lineRead.Length - 1);
                    }
                    else
                    {
                        byteArrayBuilder.Write(lineRead, 0, lineRead.Length);
                    }
                }

                byte[] receivedBytes = byteArrayBuilder.ToArray();
                return Encoding.ASCII.GetString(receivedBytes);
            }
        }

        private static bool IsLastLineInMultiLineResponse(byte[] bytesReceived)
        {
            if (bytesReceived == null)
                throw new ArgumentNullException("bytesReceived");
            return bytesReceived.Length == 1 && bytesReceived[0] == '.';
        }

        public void Dispose() 
        {
            Exit();
            if (client != null)
                client.Close();
            if (sslStream != null)
                sslStream.Close();
        }

        ~Pop3Client()
        {
            if(client != null)
            client.Close();
            if(sslStream != null)
            sslStream.Close();
        }
    }

}
