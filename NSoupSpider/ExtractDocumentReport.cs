using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSoupSpider
{
    public class ExtractDocumentReport
    {
        public Exception ExtractExcetpion { get; set; }

        public bool IsSuccess()
        {
            return ExtractExcetpion == null;
        }

    }
}
