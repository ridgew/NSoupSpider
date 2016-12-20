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
    [Serializable]
    public class ExtractElement : WorkInScopeObject
    {
        public ExtractElement(Document doc, XmlNode node, string cssQuery)
        {
            DocRoot = doc;
            NodeDefine = node;
            CssQuery = cssQuery;
        }

        public ExtractElement(Element element, XmlNode node, string cssQuery)
        {
            _containerElement = element;

            DocRoot = element.OwnerDocument;
            NodeDefine = node;

            CssQuery = cssQuery;
        }

        public Document DocRoot { get; set; }

        public XmlNode NodeDefine { get; set; }

        public string CssQuery { get; set; }


        Element _mapElement = null;
        Element _containerElement = null;

        public Element GetMapElement()
        {
            return _mapElement;
        }

        List<ExtractElement> _subElements = new List<ExtractElement>();

        public List<ExtractElement> SubExtractElements
        {
            get { return _subElements; }
        }

        public ExtractElement DataBind()
        {
            Elements _bindElements = null;
            if (_containerElement != null)
            {
                _bindElements = _containerElement.Select(CssQuery);
            }
            else
            {
                _bindElements = DocRoot.Select(CssQuery);
            }

            if (_bindElements != null && _bindElements.Count > 0)
                _mapElement = _bindElements[0];

            if (_mapElement != null)
            {
                #region 子级Element抽取
                if (NodeDefine != null && NodeDefine.ChildNodes != null && NodeDefine.ChildNodes.Count > 0)
                {
                    foreach (XmlNode subNode in NodeDefine.ChildNodes)
                    {
                        ExtractNodeDefine grabNode = new ExtractNodeDefine(subNode);
                        NodeType currentType = grabNode.GetExtractType();
                        if (currentType == NodeType.Element)
                        {
                            SubExtractElements.AddRange(grabNode.ExtractInScope(_mapElement, this.Scope));
                        }
                    }
                }
                #endregion
            }

            return this;
        }

        bool IsReturnElement()
        {
            if (NodeDefine == null) return false;

            if (NodeDefine.Attributes["return"] == null)
            {
                return NodeDefine.Attributes["retAttr"] != null
                    && string.IsNullOrEmpty(NodeDefine.Attributes["retAttr"].Value) == false

                    && NodeDefine.Attributes["name"] != null
                    && string.IsNullOrEmpty(NodeDefine.Attributes["name"].Value) == false;
            }
            else
            {
                return Convert.ToBoolean(NodeDefine.Attributes["return"].Value);
            }
        }

        public ExtractElement Extract()
        {
            if (_mapElement != null && IsReturnElement())
            {
                #region 有返回值

                XmlAttribute targetAttrs = NodeDefine.Attributes["retAttr"];
                if (targetAttrs != null && !string.IsNullOrEmpty(targetAttrs.Value))
                {
                    string[] retAttrs = targetAttrs.Value.Split(new char[] { ',', '|' });
                    if (NodeDefine.Attributes["name"] != null && !string.IsNullOrEmpty(NodeDefine.Attributes["name"].Value))
                    {
                        string[] scopeNames = NodeDefine.Attributes["name"].Value.Split(new char[] { ',', '|' });
                        if (retAttrs.Length <= scopeNames.Length)
                        {
                            for (int i = 0, j = retAttrs.Length; i < j; i++)
                            {
                                string retVal = (retAttrs[i] == "innerText") ? _mapElement.Text() : _mapElement.Attr(retAttrs[i]);
                                ExecutionContext.Current.SetValue(scopeNames[i], retVal);
                            }
                        }
                    }
                }
                #endregion
            }

            return this;
        }

    }

}
