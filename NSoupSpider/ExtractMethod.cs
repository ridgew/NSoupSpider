using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpider
{
    [Flags]
    public enum ExtractType : int
    {
        /// <summary>
        /// 节点属性
        /// </summary>
        ElementAttribute = 0,

        /// <summary>
        /// 附带条件的属性提取
        /// </summary>
        ConditionalAttribute = 1

    }

    public abstract class ExtractMethod
    {
        public bool IsCollection { get; set; }

        public abstract Dictionary<string, object> ExtractFrom(Element element);

    }

    public struct AttrMapping
    {
        public string AttrName { get; set; }

        public string MappingKey { get; set; }
    }

    /// <summary>
    /// 属性提取方式
    /// </summary>
    public class AttributeExtractMethod : ExtractMethod
    {
        /// <summary>
        /// 属性列表
        /// </summary>
        public List<AttrMapping> AttrNames { get; set; }

        public override Dictionary<string, object> ExtractFrom(Element element)
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            foreach (AttrMapping attr in AttrNames)
            {
                string retVal = string.Empty;

                if (attr.AttrName.Equals("innerText", StringComparison.InvariantCultureIgnoreCase))
                {
                    retVal = element.Text();
                }
                else
                {
                    retVal = element.Attributes[attr.AttrName];
                }

                if (!retDict.ContainsKey(attr.MappingKey))
                    retDict.Add(attr.MappingKey, retVal);
            }

            return retDict;
        }

    }


    public class CollectionExtractMethod : ExtractMethod
    {
        public CollectionExtractMethod(string collectionName, ExtractMethod itemExtract)
        {
            CollectionKey = collectionName;
            ItemExtract = itemExtract;
            IsCollection = true;
        }

        public string CollectionKey { get; set; }

        public ExtractMethod ItemExtract { get; set; }

        public override Dictionary<string, object> ExtractFrom(NSoup.Nodes.Element element)
        {
            return ItemExtract.ExtractFrom(element);
        }
    }

}
