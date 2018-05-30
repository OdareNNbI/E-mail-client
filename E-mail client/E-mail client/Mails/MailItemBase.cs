﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Pop3Client
{
    public class MailItemBase
    {

        public Dictionary<string, object> Headers { get; set; }
        public object Data { get; set; }
        public ContentType ContentType { get; set; } = new ContentType("");
        public ContentDisposition ContentDisposition { get; set; }

        public string HtmlText
        {
            get
            {
                if (htmlText != null)
                    return "<!DOCTYPE HTML><html><head><meta http-equiv = 'Content-Type' content = 'text/html;charset=UTF-8'></head><body>" + htmlText + "</body></html>";
                else
                    return "This message don't have text part";
            }
        }

        public string Text
        {
            get
            {
                if (text != null)
                    return text;
                else
                    return "This message don't have text part";
            }
        }

        private string htmlText;
        private string text;


        public MailItemBase(string source)
        {
            int headersTail = source.IndexOf("\r\n\r\n");
            string headers = String.Empty;
            if (headersTail == -1)
            {
                headers = source;
            }
            else
            {
                headers = source.Substring(0, headersTail);
            }

            this.Headers = ParseHeaders(headers);
            if (headersTail == -1)
                return;

            string body = source.Substring(headersTail + 4, source.Length - headersTail - 4);

            string contentTransferEncoding = String.Empty;
            if (this.Headers.ContainsKey("Content-Transfer-Encoding"))
            {
                contentTransferEncoding = this.Headers["Content-Transfer-Encoding"].ToString().ToLower();
            }

            if (this.Headers.ContainsKey("Content-Disposition"))
            {
                this.ContentDisposition = new ContentDisposition(this.Headers["Content-Disposition"].ToString());
            }
            if (this.Headers.ContainsKey("Content-Type"))
            {
                this.ContentType = new ContentType(this.Headers["Content-Type"].ToString());
            }
            else
            {
                this.ContentType = new ContentType("");
            }

            if (this.ContentType.Type.StartsWith("text"))
            {
                if (ContentType.Type.Contains("html"))
                {
                    htmlText = DecodeContent(contentTransferEncoding, body);
                    MailItem.HTMLTEXT = htmlText;
                }
                else if (ContentType.Type.Contains("plain"))
                {
                    text = DecodeContent(contentTransferEncoding, body);
                    MailItem.TEXT = text;
                }
            }
            else if (this.ContentType.Type.StartsWith("multipart"))
            {
                ParseMultiPart(body);
            }
            if (ContentDisposition != null && this.ContentDisposition.Type.Contains("attachment") )
            {
                this.Data = Convert.FromBase64String(body);
                FileStream stream = new FileStream("C:\\" + ContentDisposition.FileName, FileMode.OpenOrCreate);

                byte[] data = Data as byte[];
                if(data != null)
                {
                    foreach(var a in data)
                    {
                        stream.WriteByte(a);
                    }
                }

                stream.Close();
            }
            else
            {
                text = DecodeContent(contentTransferEncoding, body);
                MailItem.TEXT = text;
            }
        }

        private string HeadersEncode(Match m)
        {
            string result = String.Empty;
            Encoding cp = Encoding.GetEncoding(m.Groups["cp"].Value);
            if (m.Groups["ct"].Value.ToUpper() == "Q")
            { // кодируем из Quoted-Printable
                result = Pop3Client.DecodeQuotedPrintable(m.Groups["value"].Value);
            }
            else if (m.Groups["ct"].Value.ToUpper() == "B")
            { // кодируем из Base64
                result = cp.GetString(Convert.FromBase64String(m.Groups["value"].Value));
            }
            else
            { // такого быть не должно, оставляем текст как есть
                result = m.Groups["value"].Value;
            }
            return result;
        }

        private string ParseQuotedPrintable(string source)
        {
            source = source.Replace("_", " ");
            source = Regex.Replace(source, @"(\=)([^\dABCDEFabcdef]{2})", "");
            return Regex.Replace(source, @"\=(?<char>[\d\w]{2})", QuotedPrintableEncode);
        }

        private string QuotedPrintableEncode(Match m)
        {
            return ((char)int.Parse(m.Groups["char"].Value, System.Globalization.NumberStyles.AllowHexSpecifier)).ToString();
        }

        public Dictionary<string, object> ParseHeaders(string headers)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            headers = Regex.Replace(headers, @"([\x22]{0,1})\=\?(?<cp>[\w\d\-]+)\?(?<ct>[\w]{1})\?(?<value>[^\x3f]+)\?\=([\x22]{0,1})", HeadersEncode,
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

            headers = Regex.Replace(headers, @"([\r\n]+)^(\s+)(.*)?$", " $3", RegexOptions.Multiline);

            Regex myReg = new Regex(@"^(?<key>[^\x3A]+)\:\s{1}(?<value>.+)$", RegexOptions.Multiline);
            MatchCollection mc = myReg.Matches(headers);
            foreach (Match m in mc)
            {
                string key = m.Groups["key"].Value;
                if (result.ContainsKey(key))
                { // если указанный ключ уже есть в коллекции,
                  // то проверяем тип данных
                    if (result[key].GetType() == typeof(string))
                    { // тип данных - строка, преобразуем в коллекцию
                        ArrayList arr = new ArrayList();
                        // добавляем в коллекцию первый элемент
                        arr.Add(result[key]);
                        // добавляем в коллекцию текущий элемент
                        arr.Add(m.Groups["value"].Value);
                        // вставляем коллекцию элементов в найденный заголовок
                        result[key] = arr;
                    }
                    else
                    { // считаем, что тип данных - коллекция, 
                      // добавляем найденный элемент
                        ((ArrayList)result[key]).Add(m.Groups["value"].Value);
                    }
                }
                else
                { // такого ключа нет, добавляем
                    result.Add(key, m.Groups["value"].Value.TrimEnd("\r\n".ToCharArray()));
                }
            }


            return result;
        }

        private string DecodeContent(string contentTransferEncoding, string source)
        {
            if (contentTransferEncoding == "base64")
            {
                return ConvertCodePage(Convert.FromBase64String(source), this.ContentType.CodePage);
            }
            else if (contentTransferEncoding == "quoted-printable")
            {
                return (Pop3Client.DecodeQuotedPrintable(source, this.ContentType.CodePage.BodyName));
            }
            else
            { //"8bit", "7bit", "binary"
              // считаем, что это обычный текст
                return ConvertCodePage(source, ContentType.CodePage);
            }
        }

        private string ConvertCodePage(string source, Encoding source_encoding)
        {
            if (source_encoding == Encoding.Default) return source;
            return Encoding.Default.GetString(source_encoding.GetBytes(source));
        }

        private string ConvertCodePage(byte[] source, Encoding source_encoding)
        {
            if (source_encoding == Encoding.Default) return Encoding.Default.GetString(source);
            return Encoding.Default.GetString(Encoding.Default.GetBytes(source_encoding.GetString(source)));
        }

        private void ParseMultiPart(string b)
        {
            Regex myReg = new Regex(String.Format(@"(--{0})([^\-]{{2}})", ContentType.Boundary), RegexOptions.Multiline);
            MatchCollection mc = myReg.Matches(b);
            // создаем коллекцию частей разношерстного содержимого
            List<MailItemBase> items = new List<MailItemBase>();
            // делаем поиск каждой части сообщения
            for (int i = 0; i <= mc.Count - 1; i++)
            {
                int start = mc[i].Index + String.Format("--{0}", this.ContentType.Boundary).Length;
                int len = 0;
                if (i + 1 > mc.Count - 1)
                {
                    len = b.Length - start;
                }
                else
                {
                    len = (mc[i + 1].Index - 1) - start;
                }
                string part = b.Substring(start, len).Trim("\r\n".ToCharArray());
                int partTail = 0;
                if ((partTail = part.LastIndexOf(String.Format("--{0}--", this.ContentType.Boundary))) != -1)
                {
                    part = part.Substring(0, partTail);
                }
                items.Add(new MailItemBase(part));
            }
            // передаем коллекцию в свойство Data текущего экземпляра объекта
            this.Data = items;
        }
    }
}
