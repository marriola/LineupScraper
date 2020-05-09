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
        public HttpStatusCode StatusCode { get; private set; }

        public PageLoadException(string url, HttpStatusCode statusCode)
            : base(statusCode.ToString())
        {
            StatusCode = statusCode;
        }
    }
}
