using NSoup.Nodes;
using NSoup.Select;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpider
{
    /// <summary>
    /// 抽取数据操作
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Parent={ParentExtractElement,nq}, CssQuery={CssQuery}, Children={SubExtractElements.Count}")]
    public class ExtractElement : WorkInScopeObject
    {
        public ExtractElement(Element element, ExtractDataNode node, string cssQuery)
        {
            _containerElement = element;
            DefineNode = node;
            CssQuery = cssQuery;
        }

        public ExtractDataNode DefineNode { get; set; }

        public string CssQuery { get; set; }

        Element _mappingElement = null;

        Element _containerElement = null;

        public Element GetMapElement()
        {
            return _mappingElement;
        }

        public ExtractElement ParentExtractElement
        {
            get;
            set;
        }

        List<ExtractElement> _subElements = new List<ExtractElement>();

        public List<ExtractElement> SubExtractElements
        {
            get { return _subElements; }
        }

        public ExtractElement DataBind()
        {
            Elements _bindElements = _containerElement.Select(CssQuery); ;

            if (_bindElements != null && _bindElements.Count > 0)
                _mappingElement = _bindElements[0];

            if (_mappingElement != null && DefineNode != null)
            {
            }

            return this;
        }

        bool IsReturnElement()
        {
            if (DefineNode == null) return false;
            return DefineNode.IsReturNode();
        }

        /// <summary>
        /// 主要抽取数据处理
        /// </summary>
        /// <returns></returns>
        public ExtractElement Extract()
        {
            if (_mappingElement != null && IsReturnElement())
            {
                #region 有返回值
                //XmlAttribute targetAttrs = DefineNode.Attributes["retAttr"];
                //if (targetAttrs != null && !string.IsNullOrEmpty(targetAttrs.Value))
                //{
                //    string[] retAttrs = targetAttrs.Value.Split(new char[] { ',', '|' });
                //    if (DefineNode.Attributes["name"] != null && !string.IsNullOrEmpty(DefineNode.Attributes["name"].Value))
                //    {
                //        string[] scopeNames = DefineNode.Attributes["name"].Value.Split(new char[] { ',', '|' });
                //        if (retAttrs.Length <= scopeNames.Length)
                //        {
                //            for (int i = 0, j = retAttrs.Length; i < j; i++)
                //            {
                //                string retVal = (retAttrs[i] == "innerText") ? _mapElement.Text() : _mapElement.Attr(retAttrs[i]);
                //                ExecutionContext.Current.SetValue(scopeNames[i], retVal);
                //            }
                //        }
                //    }
                //}
                #endregion
            }

            return this;
        }


        public override string ToString()
        {
            return DefineNode.GetFullPath();
        }

    }

}
