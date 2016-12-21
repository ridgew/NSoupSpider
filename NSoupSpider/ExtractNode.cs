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
    /// 提取规则（节点）定义
    /// </summary>
    [DebuggerDisplay("NodeName={DefineNode.Name,nq}, Path={GetFullPath()}")]
    public class ExtractNode : WorkInScopeObject
    {
        public ExtractNode(XmlNode node, int deepth)
        {
            _rawNode = node;
            Deepth = deepth;
        }

        protected XmlNode _rawNode = null;

        /// <summary>
        /// 原始定义节点
        /// </summary>
        public XmlNode DefineNode
        {
            get { return _rawNode; }
        }

        /// <summary>
        /// 节点深度
        /// </summary>
        public int Deepth { get; set; }

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

            if (_rawNode.Name.Equals("dataConvert", StringComparison.InvariantCultureIgnoreCase))
            {
                return NodeType.DataConvert;
            }

            return NodeType.Element;
        }


        protected string extractIdFromNodeAttribute(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Document)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(node.Name);

            if (node.Attributes["id"] != null && !string.IsNullOrEmpty(node.Attributes["id"].Value))
            {
                sb.Append("#" + node.Attributes["id"].Value);
            }
            else
            {
                XmlNode parentNode = node.ParentNode;
                if (parentNode != null && parentNode.ChildNodes.Count > 1)
                {
                    int tagIdx = 0;
                    for (int i = 0, j = parentNode.ChildNodes.Count; i < j; i++)
                    {
                        if (parentNode.ChildNodes[i] == node && tagIdx > 0)
                        {
                            sb.Append("[" + tagIdx.ToString() + "]");
                            break;
                        }
                        else
                        {
                            if (parentNode.ChildNodes[i].Name.Equals(node.Name, StringComparison.InvariantCultureIgnoreCase))
                                tagIdx++;
                        }
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 提取规则节点路径
        /// </summary>
        /// <returns></returns>
        public string GetFullPath()
        {
            List<string> pb = new List<string>();
            if (DefineNode != null)
            {
                pb.Add(extractIdFromNodeAttribute(DefineNode));
                XmlNode parent = DefineNode.ParentNode;
                while (parent != null)
                {
                    pb.Add(extractIdFromNodeAttribute(parent));
                    parent = parent.ParentNode;
                };
            }
            return string.Join(">", pb.ToArray().Reverse());
        }


        internal XmlNodeList GetChildXmlNodes()
        {
            return _rawNode.ChildNodes;
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
        ScopeResult = 2,
        /// <summary>
        /// 数据转换
        /// </summary>
        DataConvert = 3
    }


}
