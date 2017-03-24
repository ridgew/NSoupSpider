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
    public abstract class ExtractTaskConfig
    {
        public ExtractCategory Category { get; set; }

        public abstract IExtractDocumentRule InvokeArguments { get; }

        public abstract INSoupSpiderReceiver DataReceiver { get; }
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

    /// <summary>
    /// 需要合并的数据接收器
    /// </summary>
    public interface INSoupMerginReceiver
    {
        /// <summary>
        /// 处理数据合并
        /// </summary>
        void Mergin();
    }

    public interface ISimpleObjectReceiver : INSoupSpiderReceiver
    {
        void Accept(Dictionary<string, object> resultDict);
    }

    public interface IObjectListReceiver : INSoupSpiderReceiver
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
