using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        static HtmlNode PickBand(HtmlNode disambiguation)
        {
            HtmlNodeCollection bandList = disambiguation.Element("ul").SelectNodes("descendant::li");
            for (int i = 0; i < bandList.Count; i++)
            {
                Console.Write("{0})", i + 1);
                foreach (HtmlNode node in bandList[i].ChildNodes)
                {
                    Console.Write(node.InnerText.Trim() + " ");
                }
                Console.WriteLine();
            }

            int choice = 0;
            HtmlNode bandLink;
            do
            {
                Console.Write("Please select a band: ");
                try
                {
                    choice = Convert.ToInt32(Console.ReadLine()) - 1;
                }
                catch (Exception e)
                {
                    continue;
                }
            } while (choice < 0 || choice >= bandList.Count);

            bandLink = bandList[choice].Element("a");
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
        static HtmlNode GetBandPage(string bandName)
        {
            // Retrieve the band page and check whether this is a disambiguation page.
            HtmlNode root = LoadPage("http://www.metal-archives.com/bands/" + bandName);
            HtmlNode content = root.SelectSingleNode("//div[@id='content_wrapper']");
            if (content == null)
            {
                throw new Exception("Missing content_wrapper div.");
            }
            else if (content.InnerText.Contains("\"" + bandName + "\" may refer to:"))
            {
                root = PickBand(content);
            }

            return root;
        }

        /**
         * Returns the first and last years that the band was active from the
         * band's page.
         */
        static int[] GetBandYears(HtmlNode bandPage)
        {
            int[] bandYears = new int[2] { 1970, DateTime.Today.Year };
            string startYear = "";
            string endYear = "";
            int tryYear;
            bool firstMatch = true;

            HtmlNode bandStatsDiv = bandPage.SelectSingleNode("//div[@id='band_stats']");
            HtmlNode bandYearsDd = bandStatsDiv.SelectSingleNode("descendant::dl[@class='clear']").Element("dd");
            Regex r = new Regex("(?<start>\\d+)(-(?<end>\\d+|present))?(\\(*?\\))?(,\\s*)?");
            Match match = r.Match(bandYearsDd.InnerText.Trim());

            while (match.Success)
            {
                if (firstMatch)
                {
                    startYear = match.Groups["start"].Value;
                    if (int.TryParse(startYear, out tryYear))
                    {
                        bandYears[0] = tryYear;
                    }
                }
                endYear = match.Groups["end"].Value;
                match = match.NextMatch();
                firstMatch = false;
            }

            if (int.TryParse(endYear, out tryYear))
            {
                bandYears[1] = tryYear;
            }
            return bandYears;
        }

        static List<BandMember> GetBandMembers(HtmlNode bandPage, int[] bandYears)
        {
            const string allMembersSelector = "//div[@id='band_tab_members_all']";
            const string currentMembersSelector = "//div[@id='band_tab_members_current']";
            HtmlNode bandMembersTab = bandPage.SelectSingleNode(allMembersSelector) ?? bandPage.SelectSingleNode(currentMembersSelector);
            if (bandMembersTab == null)
            {
                throw new Exception("Can't find band members.");
            }

            List<BandMember> bandMembers = new List<BandMember>();
            HtmlNodeCollection bandMemberNodes = bandMembersTab.SelectNodes("descendant::tr[@class='lineupRow']");
            foreach (HtmlNode memberEntry in bandMemberNodes)
            {
                HtmlNodeCollection columns = memberEntry.SelectNodes("descendant::td");
                string memberName = WebUtility.HtmlDecode(columns[0].Element("a").InnerText);
                string memberRole = WebUtility.HtmlDecode(columns[1].InnerText).Trim();
                bandMembers.Add(new BandMember(memberName, memberRole, bandYears[0], bandYears[1]));
            }
            return bandMembers;
        }

        static void Main(string[] args)
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

            try
            {
                HtmlNode bandPage = GetBandPage(bandName);

                Console.WriteLine("Parsing...");
                int[] bandYears = GetBandYears(bandPage);
                List<BandMember> band = GetBandMembers(bandPage, bandYears);

                Console.WriteLine("Generating timeline...");
                Timeline timeline = new Timeline(bandYears[0], bandYears[1], band);
                new FixedBlockWidthVisualizer(bandName, timeline).Save();

                Console.WriteLine("Done.");
            }
            catch (PageLoadException e)
            {
                Console.WriteLine(e.ToString());
            }
#if !DEBUG
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
#endif
            Console.ReadKey();
        }
    }
}
