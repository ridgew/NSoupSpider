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

        List<ExtractParam> extractParams = new List<ExtractParam>();

        protected override void ExtractDataByRuleMethods(Element element)
        {
            List<ExtractDataNode> cNodes = this.ChildNodes;
            if (cNodes.Count == 1)
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
                    string cssQuery = tempNode.GetCssQuery();
                    foreach (Element ele in element.Select(cssQuery))
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
                                {
                                    pageUrlList.Clear();
                                    break;
                                }
                            }
                        }
                    }

                }

                List<ExtractDataNode> paramsNodeList = tempNode.ChildNodes.Where(p => p.DefineNode.Name.Equals("params", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                if (paramsNodeList != null && paramsNodeList.Count == 1)
                {
                    ExtractDataNode paramNode = paramsNodeList[0];
                    if (paramNode.ChildNodes.Count > 0)
                    {
                        #region 绑定参数
                        if (extractParams.Count == 0)
                        {
                            //未绑定过参数
                            extractParams.AddRange(paramNode.ChildNodes.Select(d => new ExtractParam
                            {
                                Name = GetNodeNotNullAttrValue(d.DefineNode, "name"),
                                Index = Convert.ToInt32("0" + GetNodeNotNullAttrValue(d.DefineNode, "index")),
                                Scope = GetNodeNotNullAttrValue(d.DefineNode, "scope") != "workScope" ? ParamScope.arguments : ParamScope.workScope
                            }).ToList());
                        }

                        Dictionary<string, object> args = paramNode.OwnerTaskDocument.ExtractArguments;
                        extractParams.ForEach(p =>
                        {
                            if (p.Scope == ParamScope.arguments)
                            {
                                if (args.ContainsKey(p.Name))
                                    p.Value = args[p.Name].ToString();
                            }
                            else
                            {
                                p.Value = Scope.Get<string>(p.Name);
                            }
                        });
                        #endregion

                        if (extractParams.Any(p => string.IsNullOrEmpty(p.Value)) == true)
                        {
                            pageUrlList.Clear();
                        }
                        else
                        {
                            string formatOutput = string.Format(formatDef, extractParams.OrderBy(p => p.Index).Select(n => n.Value).ToArray());
                            if (!string.IsNullOrEmpty(formatOutput))
                            {
                                Scope.Set(paramName, formatOutput);
                                pageUrlList.Add(formatOutput);
                            }
                        }

                    }
                }
            }
        }

        List<string> pageUrlList = new List<string>();
        public List<string> GetPageUrlList()
        {
            return pageUrlList;
        }

    }

    public enum PagerType : int
    {
        ByNext = 0,
        PageNum = 1
    }

}
