using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpider
{
    public class ExtractPagerNode : ExtractDataNode
    {
        internal ExtractPagerNode(XmlNode node, int deepth)
            : base(node, deepth)
        {

        }

        public PagerType PageListType { get; set; }


        protected override void ExtractDataByRuleMethods(Element element)
        {
            List<ExtractDataNode> cNodes = this.ChildNodes;
            if (cNodes.Count == 1 && cNodes[0].DefineNode.Name.Equals("a", StringComparison.InvariantCultureIgnoreCase))
            {
                ExtractDataNode tempNode = cNodes[0];
                string retAttr = GetNodeNotNullAttrValue(tempNode.DefineNode, "retAttr");
                string whenDef = GetNodeNotNullAttrValue(tempNode.DefineNode, "when");
                string formatDef = GetNodeNotNullAttrValue(tempNode.DefineNode, "format");
                string opDef = GetNodeNotNullAttrValue(tempNode.DefineNode, "op");
                string opParamsDef = GetNodeNotNullAttrValue(tempNode.DefineNode, "opParams");
                string paramName = GetNodeNotNullAttrValue(tempNode.DefineNode, "paramName");

                List<ExtractDataNode> whenNodeList = tempNode.ChildNodes.Where(p => p.DefineNode.Name.Equals("when", StringComparison.InvariantCultureIgnoreCase))
                   .ToList();
                if (string.IsNullOrEmpty(whenDef) == false && whenNodeList.Count > 0)
                {
                    XmlNode whenMatchNode = whenNodeList[0].ChildNodes
                        .First(n => n.DefineNode.Name.Equals("attr", StringComparison.InvariantCultureIgnoreCase)).DefineNode;
                    string whenAttr = GetNodeNotNullAttrValue(whenMatchNode, "name");
                    string whenVal = GetNodeNotNullAttrValue(whenMatchNode, "value");

                    List<Element> matchedList = new List<Element>();
                    foreach (Element ele in element.Select("a"))
                    {
                        string rawRetVal = ele.Attr(retAttr);
                        if (string.IsNullOrEmpty(opDef) == false && string.IsNullOrEmpty(opParamsDef) == false)
                        {
                            if (opDef == "trim")
                                rawRetVal = rawRetVal.Trim(opParamsDef.ToCharArray());
                        }

                        if (whenAttr == "innerText" && whenDef == "contains")
                        {
                            if (ele.Text().Contains(whenVal))
                            {
                                Scope.Set(paramName, rawRetVal);
                                if (PageListType == PagerType.ByNext)
                                    break;
                            }
                        }
                    }

                }

                List<ExtractDataNode> paramsNodeList = tempNode.ChildNodes.Where(p => p.DefineNode.Name.Equals("params", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                if (paramsNodeList != null && paramsNodeList.Count == 1)
                {
                    ExtractDataNode paramNode = paramsNodeList[0];
                    if (paramNode.ChildNodes.Count > 0 && tempNode.IsReturNode())
                    {
                        string formatOutput = string.Format(formatDef, paramNode.ChildNodes
                            .Select(n => n.DefineNode.Attributes["name"].Value).ToArray());

                    }
                }
            }
        }
    }

    public enum PagerType : int
    {
        ByNext = 0,
        PageNum = 1
    }

}
