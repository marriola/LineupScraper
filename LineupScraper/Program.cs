using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace LineupScraper
{
    class PageLoadException : Exception
    {
        public PageLoadException(string url, HttpStatusCode code)
            : base("Page load failed: " + code.ToString())
        {
        }
    }

    class Program
    {
        /**
         * A helper function that returns the page at a URL or throws a PageLoadException.
         */
        static HtmlNode LoadPage(string url)
        {
            Console.WriteLine("Loading {0}...", url);
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            if (web.StatusCode != HttpStatusCode.OK)
            {
                throw new PageLoadException(url, web.StatusCode);
            }
            return doc.DocumentNode;
        }

        /**
         * Prompts the user to select a band from a disambiguation page.
         * 
         * @param node  HTML node containing links to band pages.
         * @return      The selected band page.
         */
        static HtmlNode PickBand(HtmlNode node)
        {
            HtmlNodeCollection bandList = node.Element("ul").SelectNodes("descendant::li");
            int i = 1;
            foreach (HtmlNode band in bandList)
            {
                Console.WriteLine("{0}) {1}", i, band.InnerText.Trim());
                i++;
            }

            Console.Write("Please select a band: ");
            int choice = Convert.ToInt32(Console.ReadLine()) - 1;
            HtmlNode bandLink = bandList[choice].Element("a");
            return LoadPage(bandLink.GetAttributeValue("href", ""));
        }

        /**
         * Loads the band page specified in the first command line argument.
         * If no band name is given, the user is prompted at the console. If a
         * search yields more than one band with the same name, the user is
         * prompted to choose one.
         * 
         * @param args  Command line arguments
         * @return      The root node of the band's Metal Archives entry
         */
        static HtmlNode GetBandPage(string[] args)
        {
            string bandName;
            if (args.Length == 0)
            {
                Console.Write("Band name? ");
                bandName = Console.ReadLine();
            }
            else
            {
                bandName = args[0];
            }

            // Retrieve the band page and check whether this is a disambiguation page.
            HtmlNode root = LoadPage("http://www.metal-archives.com/bands/" + bandName);
            HtmlNodeCollection collection = root.SelectNodes("//div[@id='content_wrapper']");
            if (collection.Count == 0)
            {
                throw new Exception("Missing content_wrapper div.");
            }
            else if (collection[0].InnerText.Contains("\"" + bandName + "\" may refer to:"))
            {
                root = PickBand(collection[0]);
            }

            return root;
        }

        static void Main(string[] args)
        {
            HtmlNode doc = GetBandPage(args);
            Console.WriteLine(doc.InnerText);
            Console.ReadKey();
        }
    }
}
