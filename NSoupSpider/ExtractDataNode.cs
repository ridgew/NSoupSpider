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
    [DebuggerDisplay("NodeName={DefineNode.Name,nq}, CssQuery={GetCssQuery(),nq}, Return={IsReturNode()}, Path={GetFullPath()}")]
    public class ExtractDataNode : ExtractNode
    {
        internal ExtractDataNode(XmlNode node, int deepth)
            : base(node, deepth)
        {
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

        public bool IsReturNode()
        {
            if (IsCollection() == false)
            {
                if (_rawNode.Attributes["return"] == null)
                {
                    return _rawNode.Attributes["retAttr"] != null
                        && string.IsNullOrEmpty(_rawNode.Attributes["retAttr"].Value) == false

                        && _rawNode.Attributes["name"] != null
                        && string.IsNullOrEmpty(_rawNode.Attributes["name"].Value) == false;
                }
                else
                {
                    return Convert.ToBoolean(_rawNode.Attributes["return"].Value);
                }
            }
            return true;
        }

        public List<ExtractMethod> GetExtractMethods()
        {
            if (!IsReturNode()) return null;

            List<ExtractMethod> methods = new List<ExtractMethod>();
            if (IsCollection() == false)
            {
                #region 非集合提取
                AttributeExtractMethod attr = new AttributeExtractMethod();
                List<AttrMapping> mapList = new List<AttrMapping>();
                XmlAttribute targetAttrs = _rawNode.Attributes["retAttr"];
                if (targetAttrs != null && !string.IsNullOrEmpty(targetAttrs.Value))
                {
                    string[] retAttrs = targetAttrs.Value.Split(new char[] { ',', '|' });
                    if (_rawNode.Attributes["name"] != null && !string.IsNullOrEmpty(_rawNode.Attributes["name"].Value))
                    {
                        string[] scopeNames = _rawNode.Attributes["name"].Value.Split(new char[] { ',', '|' });
                        if (retAttrs.Length <= scopeNames.Length)
                        {
                            for (int i = 0, j = retAttrs.Length; i < j; i++)
                            {
                                mapList.Add(new AttrMapping { AttrName = retAttrs[i], MappingKey = scopeNames[i] });
                            }
                        }
                    }
                }
                attr.AttrNames = mapList;
                methods.Add(attr);
                #endregion
            }
            else
            {
                //TODO
                string collectionKey = _rawNode.Attributes["name"].Value;
                CollectionExtractMethod colMethod = new CollectionExtractMethod(collectionKey, null);
                methods.Add(colMethod);
            }
            return methods;
        }

        public Elements ExtractElements(Element container)
        {
            string cssQuery = GetCssQuery();
            return container.Select(cssQuery);
        }

        List<ExtractDataNode> childNodes = new List<ExtractDataNode>();
        /// <summary>
        /// 0个或多个子节点
        /// </summary>
        public List<ExtractDataNode> ChildNodes
        {
            get { return childNodes; }
        }

        public static ExtractDataNode ExtractNodeAll(XmlNode node, int deepth)
        {
            ExtractDataNode eNode = new ExtractDataNode(node, deepth);
            XmlNodeList nodesList = node.ChildNodes;
            if (nodesList != null && nodesList.Count > 0)
            {
                for (int i = 0, j = nodesList.Count; i < j; i++)
                {
                    XmlNode subNode = nodesList[i];
                    eNode.ChildNodes.Add(ExtractDataNode.ExtractNodeAll(subNode, deepth + 1));
                }
            }
            return eNode;
        }


        public void ExtractDataAll(Element container)
        {
            List<ExtractMethod> fns = GetExtractMethods();
            if (fns != null && fns.Count > 0)
            {
                foreach (ExtractMethod fn in fns)
                {
                    var dict = fn.ExtractFrom(container);
                    if (dict != null)
                    {
                        foreach (var item in dict.Keys)
                        {
                            Scope.Combine(Deepth, item, dict[item]);
                        }
                    }
                }
            }

            var subElements = ExtractElements(container);
            if (subElements != null && subElements.Count > 0)
            {
                foreach (var subContainer in subElements)
                {
                    foreach (ExtractDataNode node in ChildNodes)
                    {
                        node.ExtractDataAll(subContainer);
                    }
                }
            }
        }

    }

}
