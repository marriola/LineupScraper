using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LineupScraper;

namespace RoleParserTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRoleParser()
        {
            BandMember member = new BandMember("John Doe", "Guitar, vocals (1992-2015)");
            Assert.AreEqual("John Doe: Guitar, vocals (1992-2015)", member.ToString());

            // A year range ending in "present" is just replaced with the
            // current year, since for the purposes of generating a lineup
            // chart it doesn't really matter if the member stopped that year
            // or not. BandMember.ToString's only purpose is for testing that
            // the information was parsed correctly.
            member = new BandMember("Jane Doe", "Bass (1992-1995, 2003-present), drums (1995-2003)");
            Assert.AreEqual("Jane Doe: Bass (1992-1995, 2003-2015), drums (1995-2003)", member.ToString());
        }
    }
}
