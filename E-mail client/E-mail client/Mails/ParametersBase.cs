using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pop3Client
{
    public class ParametersBase
    {
        public string Source { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public ParametersBase(string source)
        {
            if (String.IsNullOrEmpty(source)) return;
            this.Source = source;
            // ищем в источнике первое вхождение точки с запятой
            int typeTail = source.IndexOf(";");
            if (typeTail == -1)
            { // все содержимое источника является информацией о типа
                Type = source;
                return; // параметров нет, выходим
            }
            Type = source.Substring(0, typeTail);
            // парсим параметры
            string p = source.Substring(typeTail + 1, source.Length - typeTail - 1);
            this.Parameters = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            Regex myReg = new Regex(@"(?<key>.+?)=((""(?<value>.+?)"")|((?<value>[^\;]+)))[\;]{0,1}", RegexOptions.Singleline);
            MatchCollection mc = myReg.Matches(p);
            foreach (Match m in mc)
            {
                if (!this.Parameters.ContainsKey(m.Groups["key"].Value))
                {
                    string value = m.Groups["value"].Value.Trim(' ');
                    if(value.Contains('\"'))
                    {
                        value = value.Trim(new char[] { '\"' });
                        Console.Write("FF");
                    }
                    this.Parameters.Add(m.Groups["key"].Value.Trim(), value);
                }
            }
        }
    }
}
