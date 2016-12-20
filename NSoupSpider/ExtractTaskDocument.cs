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
        List<ExtractElement> _extractElements = new List<ExtractElement>();

        /// <summary>
        /// 所有需要抽取数据的节点
        /// </summary>
        public List<ExtractElement> ExtractElements
        {
            get { return _extractElements; }
        }

    }


}
