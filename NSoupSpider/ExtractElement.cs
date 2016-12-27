using NSoup.Nodes;
using NSoup.Select;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpider
{
    /// <summary>
    /// 抽取数据操作（暂时只支持属性）
    /// </summary>
    public class ExtractElement : ExtractNode
    {
        public ExtractElement(XmlNode node, int deepth)
            : base(node, deepth)
        {

        }

        public Element OperateElement { get; set; }

        public void ExtractToScope(ExtractScope Scope)
        {
            string attrName = GetNotNullAttrValue("name");
            string paramName = GetNotNullAttrValue("paramName");
            string rawReturn = OperateElement.Attr(attrName);
            if (IsOperateNode() == false)
            {
                Scope.Set(paramName, rawReturn);
            }
            else
            {
                string opType = GetNotNullAttrValue("op");
                if (opType.Equals("Regex", StringComparison.InvariantCultureIgnoreCase))
                {
                    string pattern = GetNotNullAttrValue("pattern");
                    string retVal = GetNotNullAttrValue("retVal");
                    if (string.IsNullOrEmpty(retVal))
                    {
                        int groupVal = Convert.ToInt32("0" + GetNotNullAttrValue("retGroup"));
                        Match m = Regex.Match(rawReturn, pattern);
                        if (m.Success && m.Groups.Count > groupVal)
                        {
                            Scope.Set(paramName, m.Groups[groupVal].Value);
                        }
                    }
                    else
                    {
                        Regex RE = new Regex(pattern, RegexOptions.IgnoreCase);
                        Scope.Set(paramName, RE.Replace(rawReturn, retVal));
                    }
                }
            }
        }

    }

}
