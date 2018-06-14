using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pop3Client
{
    public class ContentDisposition : ParametersBase
    {
        public string FileName
        {
            get
            {
                if (this.Parameters != null && this.Parameters.ContainsKey("filename"))
                { return this.Parameters["filename"].Replace('\n', ' ').Replace('\r', ' '); }
                return String.Empty;
            }
        }

        public ContentDisposition(string source) : base(source) { }
    }
}
