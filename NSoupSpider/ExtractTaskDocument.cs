using NSoup;
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
        internal ExtractTaskDocument()
        {

        }

        public ExtractTaskDocument(string xmlPath)
        {
            ExtractDocPath = xmlPath;

            Scope.ScopeDeepth = 1;
            Scope.ScopeId = "^";
            Scope.ContainerId = null;

        }

        /// <summary>
        /// 从接口加载抽取文档定义
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static ExtractTaskDocument FromExtractRule(IExtractDocumentRule rule)
        {
            ExtractTaskDocument doc = new ExtractTaskDocument();
            doc.Scope.ScopeDeepth = 1;
            doc.Scope.ScopeId = "^";
            doc.Scope.ContainerId = null;

            doc.SrcType = RuleLoadSource.Interface;
            ExtractScope.MergingScopeObjectWith(rule.StartupArguments(), doc.defaultArgs, true);

            XmlDocument xmlDoc = rule.RuleDocument();
            doc.parseRuleDocument(xmlDoc);
            return doc;
        }

        public RuleLoadSource SrcType { get; internal set; }

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

        internal void parseRuleDocument(XmlDocument xmlDoc)
        {
            XmlElement doc = xmlDoc.DocumentElement;
            foreach (XmlNode node in doc.ChildNodes)
            {
                ExtractNode extractNode = new ExtractNode(node, 1);
                NodeType currentType = extractNode.GetExtractType();
                if (currentType == NodeType.Element)
                {
                    _extractNodes.Add(ExtractDataNode.ExtractNodeAll(node, extractNode.Deepth, this));
                }
                else if (currentType == NodeType.UrlPattern)
                {
                    EntryUrl = UrlPatternDatasource.FromXmlNode(node);
                }
                else if (currentType == NodeType.ScopeResult)
                {
                    DocumentResult = new ScopeResult(node, extractNode.Deepth);
                }
            }
        }

        public ExtractTaskDocument BindRules()
        {
            if (SrcType != RuleLoadSource.FileSystem || !System.IO.File.Exists(ExtractDocPath))
                return this;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(ExtractDocPath);
            parseRuleDocument(xmlDoc);

            return this;
        }

        protected Dictionary<string, object> defaultArgs = new Dictionary<string, object>();

        /// <summary>
        /// 抽取参数词典
        /// </summary>
        public Dictionary<string, object> ExtractArguments
        {
            get { return defaultArgs; }
        }

        public Document GetStartupDocument()
        {
            #region 绑定运行参数
            if (ExtractArguments.Count > 0)
            {
                foreach (var key in ExtractArguments.Keys)
                {
                    ExtractParam param = EntryUrl.Params.FirstOrDefault(p => p.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                    if (param != null)
                        param.Value = ExtractArguments[key].ToString();
                }
            }
            #endregion

            if (EntryUrl == null)
            {
                throw new InvalidOperationException("EntryUrl没有绑定");
            }
            else
            {
                string srcUrl = EntryUrl.GetUrl();
                DocumentUrl = srcUrl;
                return GetDocumentByUrl(srcUrl);
            }
        }

        public Document GetDocumentByUrl(string url)
        {
            return NSoupClient.Parse(new Uri(url), 5000);
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
                }
            }
            catch (Exception extractEx)
            {
                ret.ExtractExcetpion = extractEx;
            }

            ret.CurrentExtractResult = ExtractScope.MergingAllScopeObject();

            //绑定结果（集）
            if (DocumentResult != null)
                DocumentResult.DataBind(ret.CurrentExtractResult);

            return ret;
        }

        public ExtractPagerNode GetPagerNode()
        {
            foreach (ExtractDataNode node in _extractNodes)
            {
                if (node.GetType().Equals(typeof(ExtractPagerNode)))
                    return node as ExtractPagerNode;

                ExtractPagerNode childPager = getPagerNodeFromExtractDataNode(node);
                if (childPager != null)
                    return childPager;
            }
            return null;
        }

        ExtractPagerNode getPagerNodeFromExtractDataNode(ExtractDataNode node)
        {
            foreach (ExtractDataNode subNode in node.ChildNodes)
            {
                if (subNode.GetType().Equals(typeof(ExtractPagerNode)))
                    return subNode as ExtractPagerNode;
            }
            return null;
        }

        /// <summary>
        /// 设置当前抽取文档绑定的URL地址
        /// </summary>
        public string DocumentUrl { get; set; }

    }

    public enum RuleLoadSource : int
    {
        FileSystem = 0,
        Interface = 1
    }

}
