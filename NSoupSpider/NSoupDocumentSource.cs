using NSoup;
using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpider
{
    [Serializable]
    public class NSoupDocumentSource
    {
        public NSoupDocumentSource()
        {

        }

        public NSoupDocumentSource(Uri httpUrl, int timeoutMillis)
        {
            HttpUrl = httpUrl;
            TimeoutMillis = timeoutMillis;
        }

        public Uri HttpUrl { get; set; }

        public int TimeoutMillis { get; set; }

        public List<Document> GetMatchedDocuments()
        {
            List<Document> lDocs = new List<Document>();
            lDocs.Add(NSoupClient.Parse(HttpUrl, TimeoutMillis));
            return lDocs;
        }

    }
}
