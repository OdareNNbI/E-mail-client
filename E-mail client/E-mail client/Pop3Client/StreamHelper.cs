using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pop3Client
{
    static class StreamHelper
    {
        public static byte[] ReadLineAsBytes(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            
            using (MemoryStream memoryStream = new MemoryStream())
            {
                while (true)
                {
                    int justRead = stream.ReadByte();
                    if (justRead == -1 && memoryStream.Length > 0)
                        break;

                    // Check if we started at the end of the stream we read from
                    // and we have not read anything from it yet
                    if (justRead == -1 && memoryStream.Length == 0)
                        return null;

                    char readChar = (char)justRead;

                    // Do not write \r or \n
                    if (readChar != '\r' && readChar != '\n')
                        memoryStream.WriteByte((byte)justRead);

                    // Last point in CRLF pair
                    if (readChar == '\n')
                        break;
                }

                return memoryStream.ToArray();
            }
        }

        public static string ReadLineAsString(Stream stream)
        {
            byte[] readFromStream = ReadLineAsBytes(stream);
            return readFromStream != null ? Encoding.ASCII.GetString(readFromStream) : null;
        }
    }
}
