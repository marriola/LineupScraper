using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace LineupScraperLibrary
{
    public static class LineupScraper
    {
        /// <summary>
        /// Returns a list of bands matching a search string
        /// </summary>
        /// <param name="bandName">A band name or partial band name to search</param>
        /// <returns>A list of tuples, where Item1 is a band name, and Item2 is the URL to its page</returns>
        public static List<Tuple<string,string>> SearchBands(string bandName)
        {
            var outList = new List<Tuple<string,string>>();

            HtmlNode root;
            
            try
            {
                root = LoadPage("http://www.metal-archives.com/bands/" + bandName);
            }
            catch (PageLoadException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return outList;
            }

            HtmlNode content = root.SelectSingleNode("//div[@id='content_wrapper']");
            if (content == null)
            {
                throw new Exception("Missing content_wrapper div.");
            }

            var pageBandName = root.SelectSingleNode("//h1[@class='band_name']");

            if (!content.InnerText.Contains("\"" + bandName + "\" may refer to:"))
            {
                outList.Add(Tuple.Create(pageBandName.InnerText, pageBandName.Element("a").GetAttributeValue("href", string.Empty)));
                return outList;
            }

            HtmlNodeCollection bandList = content.Element("ul").SelectNodes("descendant::li");
            foreach (HtmlNode node in bandList)
            {
                var link = node.Element("a");
                if (link != null)
                {
                    outList.Add(Tuple.Create(link.InnerText.Trim(), link.GetAttributeValue("href", string.Empty)));
                }
            }

            return outList;
        }


        /// <summary>
        /// Generates a timeline image from the band page URL provided.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Image Generate(string url)
        {
            return CreateVisualization(url).Generate();
        }


        /// <summary>
        /// Generates and saves a timeline image from the band page URL provided.
        /// Supported image formats are BMP, GIF, JPEG, PNG and TIFF.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        public static void Save(string url, string fileName)
        {
            CreateVisualization(url).Save(fileName);
        }


        #region Helper methods

        public static FixedBlockWidthVisualizer CreateVisualization(string url)
        {
            var bandPage = LoadPage(url);
            Debug.WriteLine(bandPage.InnerHtml);
            var h1 = bandPage.Descendants("h1").ToList();
            string bandName = h1[0].InnerText; //Element("//h1[@class='band_name']").InnerText;

            int[] bandYears = GetBandYears(bandPage);
            List<BandMember> band = GetBandMembers(bandPage, bandYears);

            Timeline timeline = new Timeline(bandYears[0], bandYears[1], band);
            return new FixedBlockWidthVisualizer(bandName, timeline);
        }


        /// <summary>
        /// Returns the first and last years that the band was active from the band's page.
        /// </summary>
        /// <param name="bandPage"></param>
        /// <returns></returns>
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


        /**
         * Returns the page at a URL or throws a PageLoadException.
         */
        static HtmlNode LoadPage(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            if (web.StatusCode != HttpStatusCode.OK)
            {
                throw new PageLoadException(url, web.StatusCode);
            }
            return doc.DocumentNode;
        }

        #endregion
    }
}
