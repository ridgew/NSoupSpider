using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using NSoupSpider;
using NSoup.Nodes;
using NSoup;

namespace NSoupSpiderTester
{
    [TestClass]
    public class ExtractTaskDocumentTester
    {

        //string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string baseDir = @"E:\Dev\Unit\rwgithub\NSoupSpider\NSoupSpiderTester\bin\Debug";

        [TestMethod]
        public void DocReaderSellerNameTest()
        {
            using (ExecutionContextScope scope = new ExecutionContextScope())
            {
                ExtractTaskDocument extraTask = new ExtractTaskDocument();
                string html = File.ReadAllText(Path.Combine(baseDir, @"testDocs\olp_merch_rating_3.htm"));
                Document rootDoc = NSoupClient.Parse(html);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Path.Combine(baseDir, @"rules\sellerName-uk.xml"));

                //XmlReader reader = XmlReader.Create(Path.Combine(@"E:\Dev\Error", "sellerName-uk.xml"));
                XmlElement doc = xmlDoc.DocumentElement;
                foreach (XmlNode node in doc.ChildNodes)
                {
                    ExtractNodeDefine GrabNode = new ExtractNodeDefine(node);
                    NodeType currentType = GrabNode.GetExtractType();
                    if (currentType == NodeType.Element)
                    {
                        extraTask.ExtractElements.AddRange(GrabNode.ExtractInScope(rootDoc, scope));
                    }
                }

                var context = ExecutionContext.Current;

            }
        }

        [TestMethod]
        public void DocReaderGrabBuyboxWinnerTest()
        {
            using (ExecutionContextScope scope = new ExecutionContextScope())
            {

                ExtractTaskDocument extraTask = new ExtractTaskDocument();
                string html = File.ReadAllText(Path.Combine(baseDir, @"testDocs\GrabBuyboxWinner.htm"));
                Document rootDoc = NSoupClient.Parse(html);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Path.Combine(baseDir, @"rules\GrabBuyboxWinnerHandler-uk.xml"));

                XmlElement doc = xmlDoc.DocumentElement;
                foreach (XmlNode node in doc.ChildNodes)
                {
                    ExtractNodeDefine GrabNode = new ExtractNodeDefine(node);
                    NodeType currentType = GrabNode.GetExtractType();
                    if (currentType == NodeType.Element)
                    {
                        extraTask.ExtractElements.AddRange(GrabNode.ExtractInScope(rootDoc, scope));
                    }
                }

                var context = ExecutionContext.Current;

            }
        }

        [TestMethod]
        public void DocReaderAsinImageTest()
        {
            using (ExecutionContextScope scope = new ExecutionContextScope())
            {
                ExtractTaskDocument extraTask = new ExtractTaskDocument();
                string html = File.ReadAllText(Path.Combine(baseDir, @"testDocs\GrabBuyboxWinner.htm"));
                Document rootDoc = NSoupClient.Parse(html);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Path.Combine(baseDir, @"rules\asinImage-uk.xml"));

                XmlElement doc = xmlDoc.DocumentElement;
                foreach (XmlNode node in doc.ChildNodes)
                {
                    ExtractNodeDefine GrabNode = new ExtractNodeDefine(node);
                    NodeType currentType = GrabNode.GetExtractType();
                    if (currentType == NodeType.Element)
                    {
                        extraTask.ExtractElements.AddRange(GrabNode.ExtractInScope(rootDoc, scope));
                    }
                }

                var context = ExecutionContext.Current;
            }
        }
    }
}
