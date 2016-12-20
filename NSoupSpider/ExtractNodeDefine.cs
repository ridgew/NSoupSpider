using NSoup.Nodes;
using NSoup.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpider
{
    /// <summary>
    /// 提取节点定义
    /// </summary>
    public class ExtractNodeDefine : WorkInScopeObject
    {
        public ExtractNodeDefine(XmlNode node)
        {
            _rawNode = node;
        }

        XmlNode _rawNode = null;
        public XmlNode DefineNode
        {
            get { return _rawNode; }
        }

        public bool IsCollection()
        {
            if (_rawNode.Attributes["returnCollection"] != null)
            {
                return Convert.ToBoolean(_rawNode.Attributes["returnCollection"].Value);
            }
            else if (_rawNode.Attributes["class"] != null && string.IsNullOrEmpty(_rawNode.Attributes["class"].Value) == false)
            {
                return !(_rawNode.Attributes["firstOnly"] != null && Convert.ToBoolean(_rawNode.Attributes["firstOnly"].Value));
            }
            return false;
        }

        /// <summary>
        /// 抽取数据类型
        /// </summary>
        /// <returns></returns>
        public NodeType GetExtractType()
        {
            if (_rawNode.Name.Equals("urlPattern", StringComparison.InvariantCultureIgnoreCase))
            {
                return NodeType.UrlPattern;
            }

            if (_rawNode.Name.Equals("result", StringComparison.InvariantCultureIgnoreCase))
            {
                return NodeType.ScopeResult;
            }

            return NodeType.Element;
        }

        public string GetCssQuery()
        {
            if (_rawNode == null) return string.Empty;
            bool queryElement = _rawNode.Name.Equals("element", StringComparison.InvariantCultureIgnoreCase);

            if (_rawNode.Attributes["id"] != null)
            {
                if (queryElement)
                    return string.Format("#{0}", _rawNode.Attributes["id"].Value);
                else
                    return string.Format("{0}#{1}", _rawNode.Name, _rawNode.Attributes["id"].Value);
            }
            else if (_rawNode.Attributes["class"] != null)
            {
                string clsString = _rawNode.Attributes["class"].Value.Trim();
                if (queryElement)
                {
                    return "." + clsString;
                }
                else
                {
                    return _rawNode.Name + "." + clsString;
                }
            }
            else if (_rawNode.Attributes["cssQuery"] != null)
            {
                return _rawNode.Attributes["cssQuery"].Value;
            }

            return _rawNode.Name;
        }

        public List<ExtractElement> ExtractInScope(Document doc, ExecutionContextScope scope)
        {
            List<ExtractElement> resultList = new List<ExtractElement>();
            string cssQuery = GetCssQuery();
            if (IsCollection() == false)
            {
                ExtractElement singleElement = new ExtractElement(doc, _rawNode, cssQuery);
                singleElement.Scope = scope;
                resultList.Add(singleElement.DataBind().Extract());
            }
            else
            {
                Elements eList = doc.Select(cssQuery);
                foreach (Element element in eList)
                {
                    ExtractElement item = new ExtractElement(element, _rawNode, cssQuery);
                    item.Scope = scope;
                    resultList.Add(item.DataBind().Extract());
                }
            }
            return resultList;
        }

        public List<ExtractElement> ExtractInScope(Element container, ExecutionContextScope scope)
        {
            List<ExtractElement> resultList = new List<ExtractElement>();
            string cssQuery = GetCssQuery();
            if (IsCollection() == false)
            {
                ExtractElement singleElement = new ExtractElement(container, _rawNode, cssQuery);
                singleElement.Scope = scope;
                resultList.Add(singleElement.DataBind().Extract());
            }
            else
            {
                Elements eList = container.Select(cssQuery);
                foreach (Element element in eList)
                {
                    ExtractElement item = new ExtractElement(element, _rawNode, cssQuery);
                    item.Scope = scope;
                    resultList.Add(item.DataBind().Extract());
                }
            }
            return resultList;
        }
    }

    public enum NodeType : int
    {
        /// <summary>
        /// 提取节点
        /// </summary>
        Element = 0,
        /// <summary>
        /// Url模型定义
        /// </summary>
        UrlPattern = 1,
        /// <summary>
        /// 范围结果输出
        /// </summary>
        ScopeResult = 2
    }

}
