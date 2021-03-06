﻿using NSoup.Nodes;
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

        /// <summary>
        /// 所属抽取数据文档（规则）
        /// </summary>
        public ExtractTaskDocument OwnerTaskDocument
        {
            get;
            protected set;
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

        /// <summary>
        /// 是否是返回集合的节点定义
        /// </summary>
        /// <returns></returns>
        protected bool IsReturnCollection()
        {
            if (_rawNode.Attributes["returnCollection"] != null)
            {
                return Convert.ToBoolean(_rawNode.Attributes["returnCollection"].Value);
            }
            return false;
        }

        public string GetCollectionKey()
        {
            if (_rawNode.Attributes["name"] != null)
                return _rawNode.Attributes["name"].Value;
            return extractIdFromNodeAttribute(_rawNode);
        }

        protected bool IsCollectionDescendants(out int deepth)
        {
            deepth = -1;
            ExtractDataNode parentNode = ParentExtractNode;
            while (parentNode != null)
            {
                if (parentNode.IsReturnCollection())
                {
                    deepth = parentNode.Deepth;
                    return true;
                }
                else
                {
                    parentNode = parentNode.ParentExtractNode;
                }
            }

            if (IsReturnCollection())
            {
                deepth = Deepth;    //本身定义为集合
                return true;
            }

            return false;
        }

        public bool IsReturNode()
        {
            if (IsReturnCollection() == false)
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


        public virtual List<ExtractMethod> GetExtractMethods()
        {
            if (!IsReturNode()) return null;

            List<ExtractMethod> methods = new List<ExtractMethod>();
            if (IsReturnCollection() == true && ChildNodes.Count > 0)
            {
                //TODO 通过子节点提取数据
                //string collectionKey = _rawNode.Attributes["name"] != null ? _rawNode.Attributes["name"].Value : extractIdFromNodeAttribute(_rawNode);
                //string collectionKey = _rawNode.Attributes["name"] != null ? _rawNode.Attributes["name"].Value : GetFullPath();
                //CollectionExtractMethod colMethod = new CollectionExtractMethod(collectionKey, null);
                //methods.Add(colMethod);
            }
            else
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

                #region when
                XmlAttribute whenAttr = _rawNode.Attributes["when"];
                if (whenAttr != null)
                {
                    attr.Type = ExtractType.ConditionalAttribute;
                    attr.ConditionalNode = _rawNode;
                }
                #endregion

                methods.Add(attr);
                #endregion
            }
            return methods;
        }

        public Elements ExtractContainerElements(Element container)
        {
            string cssQuery = GetCssQuery();
            return container.Select(cssQuery);
        }

        /// <summary>
        /// 父级抽取节点
        /// </summary>
        public ExtractDataNode ParentExtractNode { get; set; }


        List<ExtractDataNode> childNodes = new List<ExtractDataNode>();
        /// <summary>
        /// 0个或多个子节点
        /// </summary>
        public List<ExtractDataNode> ChildNodes
        {
            get { return childNodes; }
        }


        protected object OnOpNode(object nodeVal)
        {
            XmlAttribute attr = _rawNode.Attributes["op"];
            bool hasOp = attr != null && string.IsNullOrEmpty(attr.Value) == false;
            if (hasOp == true)
            {
                if (nodeVal == null)
                    return string.Empty;

                string opExp = attr.Value;
                if (opExp == "trim")
                {
                    char[] trimParams = _rawNode.Attributes["opParams"].Value.ToCharArray();
                    return nodeVal.ToString().Trim(trimParams);
                }
            }
            return nodeVal;
        }

        protected bool WhenNodeInContainer(Element container, Element opElement)
        {
            XmlAttribute attr = _rawNode.Attributes["when"];
            bool hasWhen = attr != null && string.IsNullOrEmpty(attr.Value) == false;
            if (hasWhen == true)
            {
                string whenExp = attr.Value;
                //TODO:条件运行
                //return opElement.Select("strong").Count == 0;
            }
            return true;
        }

        /// <summary>
        /// 只提取第一个匹配
        /// </summary>
        /// <returns></returns>
        public bool ExtractFirstOnly()
        {
            string attrVal = GetNotNullAttrValue("firstOnly");
            return !string.IsNullOrEmpty(attrVal) && Convert.ToBoolean(attrVal);
        }

        public static ExtractDataNode ExtractNodeAll(XmlNode node, int deepth, ExtractTaskDocument taskDoc)
        {
            ExtractDataNode eNode = new ExtractDataNode(node, deepth);
            eNode.OwnerTaskDocument = taskDoc;

            XmlNodeList nodesList = node.ChildNodes;
            if (nodesList != null && nodesList.Count > 0)
            {
                for (int i = 0, j = nodesList.Count; i < j; i++)
                {
                    XmlNode subNode = nodesList[i];
                    ExtractDataNode childNode = ExtractDataNode.ExtractNodeAll(subNode, deepth + 1, taskDoc);
                    string pagerAttr = GetNodeNotNullAttrValue(subNode, "isPage");
                    if (!string.IsNullOrEmpty(pagerAttr) && Convert.ToBoolean(pagerAttr))
                    {
                        //分页节点定义
                        ExtractPagerNode pagerNode = new ExtractPagerNode(subNode, deepth + 1);
                        pagerNode.OwnerTaskDocument = taskDoc;
                        pagerNode.ParentExtractNode = eNode;

                        if (childNode.ChildNodes.Count > 0)
                            pagerNode.ChildNodes.AddRange(childNode.ChildNodes);
                        eNode.childNodes.Add(pagerNode);
                    }
                    else
                    {
                        childNode.OwnerTaskDocument = taskDoc;
                        childNode.ParentExtractNode = eNode;
                        eNode.ChildNodes.Add(childNode);
                    }
                }
            }
            return eNode;
        }

        protected virtual void ExtractDataByRuleMethods(Element element)
        {
            List<ExtractMethod> fns = GetExtractMethods();
            if (fns != null && fns.Count > 0)
            {
                int collectionDeepth = -1;
                bool needPopUp = IsCollectionDescendants(out collectionDeepth);
                foreach (ExtractMethod fn in fns)
                {
                    var dict = fn.ExtractFrom(element);
                    if (dict != null)
                    {
                        foreach (var item in dict.Keys)
                        {
                            if (needPopUp == true || ParentExtractNode == null)
                            {
                                Scope.Set(item, OnOpNode(dict[item]));
                            }
                            else if (ParentExtractNode != null)
                            {
                                ParentExtractNode.Scope.Set(item, OnOpNode(dict[item]));
                            }
                        }
                    }
                }

                if (needPopUp && collectionDeepth < Deepth)
                {
                    ExtractDataNode startNode = this;
                    while (startNode != null && startNode.Deepth > collectionDeepth)
                    {
                        startNode.Scope.PopUp(true);
                        startNode = startNode.ParentExtractNode;
                    }
                }
            }
        }

        void ExtractChild(Element container)
        {
            foreach (ExtractDataNode node in ChildNodes)
            {
                if (node.Scope.ContainerScope == null)
                    node.Scope.ContainerScope = Scope;

                if (node.DefineNode.Name.Equals("attrs", StringComparison.InvariantCultureIgnoreCase))
                {
                    //属性提取
                    ExtractElement extract = new ExtractElement(node.DefineNode, node.Deepth);
                    extract.OperateElement = container;
                    extract.ExtractToScope(Scope);
                }
                else if (node.DefineNode.Name.Equals("when", StringComparison.InvariantCultureIgnoreCase))
                {
                    //条件提取
                }
                else if (node.DefineNode.Name.Equals("params", StringComparison.InvariantCultureIgnoreCase))
                {
                    //变量（参数）绑定
                }
                else
                {
                    XmlAttribute whenAttr = node.DefineNode.Attributes["when"];
                    if (whenAttr == null)
                    {
                        node.ExtractDataAll(container);
                    }
                    else
                    {
                        string whenExp = whenAttr.Value;
                    }
                }
            }
        }

        public void ExtractDataAll(Element container)
        {
            var allMatchElements = ExtractContainerElements(container);
            if (allMatchElements != null && allMatchElements.Count > 0)
            {
                if (IsReturnCollection())
                {
                    string collectKey = GetCollectionKey();
                    Scope.ScopeId = collectKey;

                    #region 集合直接处理子级
                    List<Dictionary<string, Object>> colist = new List<Dictionary<string, object>>();
                    foreach (var subContainer in allMatchElements)
                    {
                        Dictionary<string, Object> collectionItem = new Dictionary<string, object>();
                        if (childNodes.Count == 0)
                        {
                            ExtractDataByRuleMethods(subContainer);
                            Dictionary<string, Object> nodeScopeObj = Scope.GetContainerObjectIteration();
                            ExtractScope.MergingScopeObjectWith(nodeScopeObj, collectionItem, true);
                        }
                        else
                        {
                            foreach (ExtractDataNode node in ChildNodes)
                            {
                                node.Scope.ContainerScope = Scope;
                                node.ExtractDataAll(subContainer);
                                Dictionary<string, Object> nodeScopeObj = node.Scope.GetContainerObjectIteration();
                                ExtractScope.MergingScopeObjectWith(nodeScopeObj, collectionItem, true);
                            }
                        }
                        colist.Add(collectionItem);
                    }
                    Scope.Set(collectKey, colist);
                    #endregion
                }
                else
                {
                    foreach (var element in allMatchElements)
                    {
                        #region 本级处理
                        if (WhenNodeInContainer(container, element) != false)
                            ExtractDataByRuleMethods(element);
                        #endregion

                        #region 子级处理
                        ExtractChild(element);
                        #endregion

                        if (ExtractFirstOnly() == true)
                            break;
                    }
                }
            }
        }

    }

}
