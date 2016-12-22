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
        //string baseDir = @"D:\DevRoot\NSoupSpider\NSoupSpiderTester\bin\Debug";
        string baseDir = @"E:\Dev\Unit\rwgithub\NSoupSpider\NSoupSpiderTester\bin\Debug";

        [TestMethod]
        public void DocReaderSellerNameTest()
        {
            TestDocumentWithRules(Path.Combine(baseDir, @"testDocs\olp_merch_rating_3.htm"), Path.Combine(baseDir, @"rules\sellerName-uk.xml"));
        }

        void TestDocumentWithRules(string docPath, string rulePath)
        {
            string html = File.ReadAllText(docPath);
            Document rootDoc = NSoupClient.Parse(html);
            using (ExecutionContextScope scope = new ExecutionContextScope())
            {
                ExtractTaskDocument extraTask = new ExtractTaskDocument(rulePath).BindRules();
                ExtractDocumentReport report = extraTask.ExtractWith(rootDoc);

                if (!report.IsSuccess())
                {
                    throw report.ExtractExcetpion;
                }

                Dictionary<string, object> objRsult = ExtractScope.MergingAllScopeObject();
                ExecutionContext context = ExecutionContext.Current;

            }

        }

        [TestMethod]
        public void DocReaderGrabBuyboxWinnerTest()
        {
            TestDocumentWithRules(Path.Combine(baseDir, @"testDocs\GrabBuyboxWinner.htm"), Path.Combine(baseDir, @"rules\GrabBuyboxWinnerHandler-uk.xml"));
        }

        [TestMethod]
        public void DocReaderAsinImageTest()
        {
            TestDocumentWithRules(Path.Combine(baseDir, @"testDocs\GrabBuyboxWinner.htm"), Path.Combine(baseDir, @"rules\asinImage-uk.xml"));
        }

        [TestMethod]
        public void GrabOfferListingsHandlerTest()
        {
            TestDocumentWithRules(Path.Combine(baseDir, @"testDocs\offer-listing-B009A4A8ZO.htm"), Path.Combine(baseDir, @"rules\GrabOfferListingsHandler-uk.xml"));
        }

    }
}
