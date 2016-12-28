using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpider
{
    /// <summary>
    /// 抽取任务配置
    /// </summary>
    [Serializable]
    public class ExtractTaskConfig
    {
        public ExtractCategory Category { get; set; }

        public IExtractDocumentRule InvokeArguments { get; set; }

        public INSoupSpiderReceiver DataReceiver { get; set; }
    }

    public enum ExtractCategory : int
    {
        SimpleObject = 0,
        ObjectList = 1,
        Mixed = 2
    }

    public interface IExtractDocumentRule
    {
        System.Xml.XmlDocument RuleDocument();

        Dictionary<string, object> StartupArguments();
    }

    #region 接收接口实现
    [Serializable]
    public class MixedExtractData
    {

        public Dictionary<string, object> Summary { get; set; }


        public Dictionary<string, List<Dictionary<string, object>>> NamedListObject { get; set; }
    }

    public interface INSoupSpiderReceiver
    {

    }

    public interface ISimpleObjectReceiver : INSoupSpiderReceiver
    {
        void Accept(Dictionary<string, object> resultDict);
    }

    public interface IObjectListReceiver : ISimpleObjectReceiver
    {
        int RecordCount { get; set; }

        /// <summary>
        /// 返回当前记录的索引
        /// </summary>
        /// <param name="resultDict">单个对象词典</param>
        /// <returns></returns>
        int Send(Dictionary<string, object> resultDict);

    }

    public interface IMixedDataReceiver : INSoupSpiderReceiver
    {
        void Accept(MixedExtractData data);
    }
    #endregion

}
