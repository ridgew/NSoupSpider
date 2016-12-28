using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NSoupSpider
{
    public class ScopeResult : ExtractNode
    {
        internal ScopeResult(XmlNode node, int deepth)
            : base(node, deepth)
        {
            initialItemList();
        }

        void initialItemList()
        {
            XmlNodeList nodes = GetChildXmlNodes();
            foreach (XmlNode node in nodes)
            {
                itemList.Add(new ResultItem
                {
                    ItemName = GetNodeNotNullAttrValue(node, "name"),
                    Value = GetNodeNotNullAttrValue(node, "value"),
                    WhenExpresion = GetNodeNotNullAttrValue(node, "when"),
                    MappingName = GetNodeNotNullAttrValue(node, "applyFor")
                });
            }
        }

        List<ResultItem> itemList = new List<ResultItem>();

        Dictionary<string, object> scopeResult = new Dictionary<string, object>();

        public void DataBind(Dictionary<string, object> result)
        {
            ExtractScope.MergingScopeObjectWith(result, scopeResult, true);
        }

        public T Get<T>(string itemName)
        {
            object retVal = null;

            ResultItem itemSet = itemList.FirstOrDefault(t => t.ItemName.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));
            if (itemSet != null)
            {
                if (string.IsNullOrEmpty(itemSet.MappingName) == false)
                    itemName = itemSet.MappingName;

                if (itemSet.Value != null && itemSet.Value.ToString() != string.Empty)
                {
                    return (T)itemSet.Value;
                }
            }

            if (scopeResult.ContainsKey(itemName))
                retVal = scopeResult[itemName];

            if (retVal != null)
                return (T)Convert.ChangeType(retVal, typeof(T));

            return default(T);
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "item")]
    public class ResultItem
    {

        [XmlAttribute(AttributeName = "name")]
        public string ItemName { get; set; }

        /// <summary>
        /// 映射名称（可选）
        /// </summary>
        [XmlAttribute(AttributeName = "applyFor")]
        public string MappingName { get; set; }

        /// <summary>
        /// 固定值设置（默认值）
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public object Value { get; set; }

        /// <summary>
        /// 条件表达式
        /// </summary>
        [XmlAttribute(AttributeName = "when")]
        public string WhenExpresion { get; set; }
    }

}
