using NSoupSpider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpiderTester
{
    public class WishColorSyncTask : ExtractTaskConfig
    {
        WishColorSyncReceiver receiver = new WishColorSyncReceiver();

        public override INSoupSpiderReceiver DataReceiver
        {
            get { return receiver; }
        }

        WishColorSyncRule theRule = new WishColorSyncRule();

        public override IExtractDocumentRule InvokeArguments
        {
            get { return theRule; }
        }

    }

    public class WishColorSyncReceiver : IObjectListReceiver
    {
        int _recIndex = 0;
        public int RecordCount { get; set; }

        public int Send(Dictionary<string, object> resultDict)
        {
            string singleColorKey = "color";
            if (resultDict.ContainsKey(singleColorKey))
            {
                string acceptColor = resultDict["color"].ToString();
                //Store Color Here!!!
            }
            return _recIndex++;
        }
    }

    public class WishColorSyncRule : IExtractDocumentRule
    {

        public System.Xml.XmlDocument RuleDocument()
        {
            string cachSettingStr = GetInitialRuleXml();
            System.Xml.XmlDocument retDoc = new System.Xml.XmlDocument();
            retDoc.LoadXml(cachSettingStr);
            return retDoc;
        }

        Dictionary<string, object> args = new Dictionary<string, object>();
        public Dictionary<string, object> StartupArguments()
        {
            return args;
        }

        public string GetInitialRuleXml()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                <root>
                <UrlPattern format=""https://merchant.wish.com/documentation/colors"" />
                <div id=""documentation-products-content"">
                    <span class=""documentation-content"">
	                   <ul name=""colorList"">
                          <li returnCollection=""true"" retAttr=""innerText"" name=""color"" scope=""new"" />
                       </ul>
                    </span>
                </div>
                </root>";
        }

    }
}
