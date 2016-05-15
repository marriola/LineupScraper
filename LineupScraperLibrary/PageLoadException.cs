using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraperLibrary
{
    public class PageLoadException : Exception
    {
        public PageLoadException(string url, HttpStatusCode code)
            : base("Page load failed: " + code.ToString())
        {
        }
    }
}
