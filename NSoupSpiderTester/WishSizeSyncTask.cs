using NSoupSpider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpiderTester
{
    public class WishSizeSyncTask : ExtractTaskConfig
    {
        WishSizeSyncReceiver receiver = new WishSizeSyncReceiver();

        public override INSoupSpiderReceiver DataReceiver
        {
            get { return receiver; }
        }

        WishSizeSyncRule theRule = new WishSizeSyncRule();

        public override IExtractDocumentRule InvokeArguments
        {
            get { return theRule; }
        }
    }

    public class WishSizeSyncRule : IExtractDocumentRule
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
                <UrlPattern format=""https://merchant.wish.com/documentation/sizes"" />
                <div id=""documentation-products-content"">
                    <span class=""documentation-content"">
	                   <h4 name=""category"" returnCollection=""true"" retAttr=""innerText""  scope=""new"" />
	                   <ul name=""catSize"" returnCollection=""true""  scope=""new"">
                           <li returnCollection=""true"" retAttr=""innerText"" name=""size"" scope=""new"" when=""$nexists(strong)"" />
                       </ul>
                    </span>
                </div>
                </root>";
        }

    }

    public class WishSizeSyncReceiver : IObjectListReceiver, INSoupMerginReceiver
    {
        public int RecordCount { get; set; }

        Dictionary<string, int> recIdxDict = new Dictionary<string, int>();

        Dictionary<int, string> CateIdxDict = new Dictionary<int, string>();
        Dictionary<int, object> SizeIdxDict = new Dictionary<int, object>();

        public int Send(Dictionary<string, object> resultDict)
        {
            //category catSize size
            string key = "testSendKeys";
            if (resultDict.ContainsKey("category"))
            {
                key = "category";
                int idx = recIdxDict.ContainsKey(key) ? recIdxDict[key] + 1 : 0;
                CateIdxDict.Add(idx, resultDict[key].ToString());
            }
            else if (resultDict.ContainsKey("size"))
            {
                key = "size";
                int idx = recIdxDict.ContainsKey(key) ? recIdxDict[key] + 1 : 0;
                SizeIdxDict.Add(idx, resultDict[key]);
            }
            else if (resultDict.ContainsKey("catSize"))
            {

            }

            if (recIdxDict.ContainsKey(key) == false)
            {
                recIdxDict.Add(key, 0);
            }
            else
            {
                recIdxDict[key] = recIdxDict[key] + 1;
            }
            return recIdxDict[key];
        }

        public void Mergin()
        {
            foreach (var key in CateIdxDict.Keys)
            {
                string category = CateIdxDict[key];

                if (category == "Custom Size")
                    continue;

                string sizeKey = "size";
                if (SizeIdxDict.ContainsKey(key))
                {
                    List<Dictionary<string, object>> sizeList = SizeIdxDict[key] as List<Dictionary<string, object>>;
                    if (sizeList != null)
                    {
                        foreach (var sizeObj in sizeList)
                        {
                            if (sizeObj.ContainsKey(sizeKey))
                            {
                                string sizeItem = sizeObj[sizeKey].ToString();
                                //TODO: sync sizeItem of category
                            }
                        }
                    }
                }
            }
        }
    }
}