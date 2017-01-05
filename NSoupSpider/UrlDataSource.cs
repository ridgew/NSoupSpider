using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;

namespace NSoupSpider
{
    public abstract class UrlDataSource
    {
        public abstract string GetUrl();
    }


    public class UrlPatternDatasource : UrlDataSource
    {
        public UrlPatternDatasource(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; set; }

        List<ExtractParam> _params = new List<ExtractParam>();

        public List<ExtractParam> Params
        {
            get { return _params; }
        }

        public override string GetUrl()
        {
            string idxPattern = "\\{\\d+\\}";
            if (Regex.IsMatch(Pattern, idxPattern))
            {
                return Regex.Replace(Pattern, idxPattern, m =>
                {
                    int idx = Convert.ToInt32(m.Value.TrimStart('{').TrimEnd('}'));
                    if (idx >= 0 && idx < Params.Count)
                        return Params[idx].Value;
                    else
                        return string.Empty;
                });
            }
            return Pattern;
        }

        public static UrlPatternDatasource FromXmlNode(XmlNode node)
        {
            XmlAttribute attrFormat = node.Attributes["format"];
            XmlAttribute attrExample = node.Attributes["example"];
            if (attrFormat == null && attrExample != null)
            {
                return new UrlPatternDatasource(attrExample.Value);
            }

            string format = attrFormat.Value;
            UrlPatternDatasource ret = new UrlPatternDatasource(format);

            int idx = 0;
            foreach (XmlNode sub in node.ChildNodes)
            {
                if (sub.Name.Equals("param", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Params.Add(new ExtractParam
                    {
                        Index = idx,
                        Scope = ParamScope.arguments,
                        Name = sub.Attributes["name"].Value
                    });
                    idx++;
                }
            }

            return ret;
        }

    }
}
