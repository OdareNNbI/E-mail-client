using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pop3Client
{
    public class ContentType :ParametersBase
    {
        public string Charset
        {
            get
            {
                if (this.Parameters != null && this.Parameters.ContainsKey("charset"))
                { return this.Parameters["charset"].Replace(" ",""); }
                return "utf-8"; // по умолчанию 
            }
        }
        public string Boundary
        {
            get
            {
                if (this.Parameters != null && this.Parameters.ContainsKey("boundary"))
                { return this.Parameters["boundary"]; }
                return String.Empty;
            }
        }
        public string Format
        {
            get
            {
                if (this.Parameters != null && this.Parameters.ContainsKey("format"))
                { return this.Parameters["format"]; }
                return String.Empty;
            }
        }
        private Encoding _CodePage = null;
        public Encoding CodePage
        {
            get
            {
                if (_CodePage == null && !String.IsNullOrEmpty(this.Charset))
                { _CodePage = Encoding.GetEncoding(this.Charset); }
                else
                { _CodePage = Encoding.UTF8; }
                return _CodePage;
            }
        }
        public ContentType(string source) : base(source) { }
    }
}
