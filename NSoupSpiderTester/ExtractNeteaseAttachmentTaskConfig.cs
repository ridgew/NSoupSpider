using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSoupSpider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NSoupSpiderTester
{
    public class ExtractNeteaseAttachmentTaskConfig : ExtractTaskConfig
    {
        public ExtractNeteaseAttachmentTaskConfig()
        {
            base.Category = ExtractCategory.ObjectList;
        }

        ExtractNeteaseAttachmentRule rule = new ExtractNeteaseAttachmentRule();
        public override IExtractDocumentRule InvokeArguments
        {
            get { return rule; }
        }

        ExtractNeteaseAttachmentReceiver receiver = new ExtractNeteaseAttachmentReceiver();
        public override INSoupSpiderReceiver DataReceiver
        {
            get { return receiver; }
        }

    }

    public class ExtractNeteaseAttachmentRule : IExtractDocumentRule
    {
        public XmlDocument RuleDocument()
        {
            string ruleXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<root>
<UrlPattern example=""E:\Dev\Unit\rwgithub\NSoupSpider\NSoupSpiderTester\testDocs\netease-attatchment.htm"" />
<div id=""divNeteaseBigAttach"">
	<div cssQuery=""[style*='clear:both;height:36px;padding:6px 4px']"" returnCollection=""true"" name=""imgList"" scope=""new"">
		<div cssQuery=""[style*='padding:0px;font-size:12px;line-height:14px']"">
           <a retAttr=""innerText,download"" name=""ImageName,DownLoadPage"" />
        </div>
	</div>
</div>
<result>
	<item name=""ImageName"" />
	<item name=""DownLoadPage"" />
</result>
</root>";


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ruleXml);

            return xmlDoc;
        }

        Dictionary<string, object> args = new Dictionary<string, object>();
        public Dictionary<string, object> StartupArguments()
        {
            return args;
        }
    }

    public class ExtractNeteaseAttachmentReceiver : IObjectListReceiver
    {
        public ExtractNeteaseAttachmentReceiver()
        {
            netClient.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:50.0) Gecko/20100101 Firefox/50.0");
            netClient.Headers.Set("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            netClient.Headers.Set("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            //netClient.Headers.Set("Accept-Encoding", "gzip, deflate");
        }

        HttpClient netClient = new HttpClient();

        int currentIdx = 0;

        public int RecordCount { get; set; }

        public int Send(Dictionary<string, object> resultDict)
        {
            //if (currentIdx >= 1)
            //    return currentIdx++;

            Accept(resultDict);
            return currentIdx++;
        }

        public void Accept(Dictionary<string, object> resultDict)
        {
            string ImageName = resultDict["ImageName"].ToString();
            string urlPage = resultDict["DownLoadPage"].ToString();

            string imgSaveDir = @"E:\Dev\Unit\rwgithub\NSoupSpider\TestImages\";
            string fileHtml = netClient.DownloadString(urlPage);
            string startKey = "downloadlink = '";
            int fileUrlIdx = fileHtml.IndexOf(startKey);
            if (fileUrlIdx != -1)
            {
                int urlEndIdx = fileHtml.IndexOf("';", fileUrlIdx);
                if (urlEndIdx > fileUrlIdx)
                {
                    fileUrlIdx += startKey.Length;
                    string realUrl = fileHtml.Substring(fileUrlIdx, urlEndIdx - fileUrlIdx);
                    netClient.DownloadFile(realUrl, imgSaveDir + ImageName);
                }
            }
            System.Threading.Thread.Sleep(500);
        }
    }

    [TestClass]
    public class ExtractNATester
    {

        [TestMethod]
        public void GetImgListTest()
        {
            ExtractNeteaseAttachmentTaskConfig cfg = new ExtractNeteaseAttachmentTaskConfig();
            SpiderAgent.Execute(cfg);
        }


    }
}
