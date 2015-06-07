﻿using System;
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

        static List<BandMember> GetBandMembers(HtmlNode bandPage)
        {
            HtmlNode bandMembersTab = bandPage.SelectSingleNode("//div[@id='band_tab_members_all']");
            if (bandMembersTab == null)
            {
                bandMembersTab = bandPage.SelectSingleNode("//div[@id='band_tab_members_current']");
                if (bandMembersTab == null)
                {
                    throw new Exception("Can't find band members.");
                }
            }

            List<BandMember> bandMembers = new List<BandMember>();
            HtmlNodeCollection bandMemberNodes = bandMembersTab.SelectNodes("descendant::tr[@class='lineupRow']");
            foreach (HtmlNode memberEntry in bandMemberNodes)
            {
                HtmlNodeCollection columns = memberEntry.SelectNodes("descendant::td");
                string memberName = WebUtility.HtmlDecode(columns[0].Element("a").InnerText);
                string memberRole = WebUtility.HtmlDecode(columns[1].InnerText).Trim();
                bandMembers.Add(new BandMember(memberName, memberRole));
            }
            return bandMembers;
        }

        static void Main(string[] args)
        {
            try
            {
                HtmlNode bandPage = GetBandPage(args);
                List<BandMember> bandMembers = GetBandMembers(bandPage);
                foreach (BandMember member in bandMembers)
                {
                    Console.WriteLine(member.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }
    }
}
