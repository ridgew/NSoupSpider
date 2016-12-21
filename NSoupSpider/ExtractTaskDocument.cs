using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpider
{

    [Serializable]
    public class ExtractTaskDocument : WorkInScopeObject
    {
        public ExtractTaskDocument(string xmlPath)
        {
            ExtractDocPath = xmlPath;
        }

        public string ExtractDocPath { get; set; }


        public UrlPatternDatasource EntryUrl { get; set; }

        /// <summary>
        /// 文档级别的结果输出
        /// </summary>
        public ScopeResult DocumentResult { get; protected set; }


        List<ExtractDataNode> _extractNodes = new List<ExtractDataNode>();
        /// <summary>
        /// 所有需要抽取数据的节点定义
        /// </summary>
        public List<ExtractDataNode> ExtractDataNodes
        {
            get { return _extractNodes; }
        }

        void parseRuleDocument(XmlDocument xmlDoc)
        {
            XmlElement doc = xmlDoc.DocumentElement;
            foreach (XmlNode node in doc.ChildNodes)
            {
                ExtractNode extractNode = new ExtractNode(node, 1);
                NodeType currentType = extractNode.GetExtractType();
                if (currentType == NodeType.Element)
                {
                    _extractNodes.Add(ExtractDataNode.ExtractNodeAll(node, extractNode.Deepth));
                }
                else if (currentType == NodeType.UrlPattern)
                {
                    EntryUrl = UrlPatternDatasource.FromXmlNode(node);
                }
                else if (currentType == NodeType.ScopeResult)
                {
                    DocumentResult = new ScopeResult();
                }
            }
        }

        public ExtractTaskDocument BindRules()
        {
            if (!System.IO.File.Exists(ExtractDocPath))
                return this;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(ExtractDocPath);
            parseRuleDocument(xmlDoc);

            return this;
        }

        /// <summary>
        /// 无异常处理
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public ExtractDocumentReport ExtractWith(Document doc)
        {
            ExtractDocumentReport ret = new ExtractDocumentReport();
            try
            {
                using (ExecutionContextScope scope = new ExecutionContextScope())
                {
                    foreach (ExtractDataNode _node in _extractNodes)
                    {
                        _node.ExtractDataAll(doc);
                    }

                    //TODO绑定结果（集）
                    if (DocumentResult != null)
                    {

                    }
                }
            }
            catch (Exception extractEx)
            {
                ret.ExtractExcetpion = extractEx;
            }
            return ret;
        }

    }






}
