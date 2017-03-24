using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;

namespace NSoupSpider
{
    [Flags]
    public enum ExtractType : int
    {
        /// <summary>
        /// 节点属性
        /// </summary>
        ElementAttribute = 0,

        /// <summary>
        /// 附带条件的属性提取
        /// </summary>
        ConditionalAttribute = 1

    }

    public abstract class ExtractMethod
    {
        ExtractType _type = ExtractType.ElementAttribute;
        public ExtractType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public bool IsCollection { get; set; }

        /// <summary>
        /// 条件定义节点
        /// </summary>
        public System.Xml.XmlNode ConditionalNode { get; set; }

        public abstract Dictionary<string, object> ExtractFrom(Element element);

    }

    public struct AttrMapping
    {
        public string AttrName { get; set; }

        public string MappingKey { get; set; }
    }

    /// <summary>
    /// 属性提取方式
    /// </summary>
    public class AttributeExtractMethod : ExtractMethod
    {
        /// <summary>
        /// 属性列表
        /// </summary>
        public List<AttrMapping> AttrNames { get; set; }

        public override Dictionary<string, object> ExtractFrom(Element element)
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            if (element != null)
            {
                if (Type == ExtractType.ConditionalAttribute && ConditionalNode != null)
                {
                    XmlAttribute attr = ConditionalNode.Attributes["when"];
                    bool hasWhen = attr != null && string.IsNullOrEmpty(attr.Value) == false;
                    if (hasWhen == true)
                    {
                        string whenExp = attr.Value;
                        #region TODO:条件运行
                        //$exists(strong) $nexists(strong)
                        string knownExp = "\\$(\\w+)\\((\\w+)\\)";
                        Match m = Regex.Match(whenExp, knownExp, RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            string method = m.Groups[1].Value;
                            string childElementName = m.Groups[2].Value;
                            if (method == "nexists")
                            {
                                //不存在
                                Predicate<Element> match = new Predicate<Element>(t => t.Select(childElementName).Count == 0);
                                if (match(element) == false)
                                    goto retResult;
                            }
                            else if (method == "exists")
                            {
                                //存在
                                Predicate<Element> match = new Predicate<Element>(t => t.Select(childElementName).Count > 0);
                                if (match(element) == false)
                                    goto retResult;
                            }
                        }
                        #endregion
                    }
                }

                #region 提取属性数据
                foreach (AttrMapping attr in AttrNames)
                {
                    string retVal = string.Empty;
                    if (attr.AttrName.Equals("innerText", StringComparison.InvariantCultureIgnoreCase))
                    {
                        retVal = element.Text();
                    }
                    else
                    {
                        retVal = element.Attributes[attr.AttrName];
                    }
                    if (!retDict.ContainsKey(attr.MappingKey))
                        retDict.Add(attr.MappingKey, retVal);
                }
                #endregion
            }

        retResult:
            return retDict;
        }

    }


    public class CollectionExtractMethod : ExtractMethod
    {
        public CollectionExtractMethod(string collectionName, ExtractMethod itemExtract)
        {
            CollectionKey = collectionName;
            ItemExtract = itemExtract;
            IsCollection = true;
        }

        public string CollectionKey { get; set; }

        public ExtractMethod ItemExtract { get; set; }

        public override Dictionary<string, object> ExtractFrom(Element element)
        {
            //TODO
            if (ItemExtract != null)
                return ItemExtract.ExtractFrom(element);
            else
                return null;
        }
    }

}
