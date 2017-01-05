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
        string baseDir = @"E:\Dev\proj\EnterpriseCommon\NSoupSpider\NSoupSpiderTester\bin\Debug";

        [TestMethod]
        public void DocReaderSellerNameTest()
        {
            TestDocumentWithRules(Path.Combine(baseDir, @"testDocs\olp_merch_rating_3.htm"), Path.Combine(baseDir, @"rules\sellerName-uk.xml"));
        }

        void TestDocumentWithRules(string docPath, string rulePath)
        {
            ExtractTaskDocument taskDoc = new ExtractTaskDocument(rulePath).BindRules();
            taskDoc.ExtractArguments.Add("SiteUrl", "https://www.amazon.co.uk");
            taskDoc.ExtractArguments.Add("Asin", "B009A4A8ZO");
            taskDoc.ExtractArguments.Add("SellerId", "APZXIW0LSZ4VN");

            //Document rootDoc = extraTask.GetStartupDocument();
            string html = File.ReadAllText(docPath);
            Document rootDoc = NSoupClient.Parse(html);


        fetchPageData:
            using (ExecutionContextScope scope = new ExecutionContextScope())
            {
                ExtractDocumentReport report = taskDoc.ExtractWith(rootDoc);
                if (!report.IsSuccess())
                {
                    throw report.ExtractExcetpion;
                }
                else
                {
                    ExtractPagerNode node = taskDoc.GetPagerNode();
                    if (node != null)
                    {
                        List<string> nextUrls = node.GetPageUrlList();
                        if (node.PageListType == PagerType.ByNext)
                        {
                            if (nextUrls.Any())
                            {
                                taskDoc.DocumentUrl = nextUrls[0];
                                rootDoc = taskDoc.GetDocumentByUrl(taskDoc.DocumentUrl);
                                goto fetchPageData;
                            }
                        }
                        else
                        {
                            string currentDocUrl = taskDoc.EntryUrl.GetUrl();
                        }
                    }
                }

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
