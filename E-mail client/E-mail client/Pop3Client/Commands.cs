using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    public static class GmailSettings
    {
        public const string host = "pop.gmail.com";
        public const int port = 995;
    }

    public static class RequestCommands
    {
        public const string USER = "USER ";
        public const string PASSWORD = "PASS ";
        public const string RETURN_MESSAGE = "RETR ";
        public const string QUIT = "QUIT ";
        public const string CHECK_CONNECTION = "NOOP";
        public const string UIDL_OF_MESSAGE = "UIDL ";
    }

    public static class ResponseCommands
    {
        public const string OK = "+OK";
        public const string ERROR = "-ERR";
    }
}
