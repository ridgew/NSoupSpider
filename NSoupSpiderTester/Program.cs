using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSoup;
using NSoupSpider;
using NSoup.Nodes;
using NSoup.Select;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NSoupSpiderTester
{
    [TestClass]
    public class Program
    {
        static void Main(string[] args)
        {
            //NSoupDocumentSource dSrc = new NSoupDocumentSource(new Uri("https://www.amazon.co.uk/gp/offer-listing/B009A4A8ZO"), 3000);
            //Document doc = dSrc.GetMatchedDocuments().First();

            string html = System.IO.File.ReadAllText(@"E:\Dev\Unit\rwgithub\NSoupSpider\NSoupSpiderTester\testDocs\offer-listing-B009A4A8ZO.htm");
            Document doc = NSoupClient.Parse(html);

            Elements aEles = doc.Select("#olpOfferListColumn");
            if (aEles != null && aEles.Any())
            {
                Elements internalElements = aEles.First().Select(".a-row");
            }

        }

        [TestMethod]
        public void WishColorSyncTest()
        {
            WishColorSyncTask cfg = new WishColorSyncTask();
            try
            {
                SpiderAgent.Execute(cfg);
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void WishSizeSyncTest()
        {
            WishSizeSyncTask cfg = new WishSizeSyncTask();
            try
            {
                SpiderAgent.Execute(cfg);
            }
            catch (Exception)
            {

            }
        }

    }
}
