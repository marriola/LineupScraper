using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LineupScraper;

namespace RoleParserTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestYears()
        {
            BandMember member = new BandMember("John Doe", "Guitar, vocals (1992-2015)");
            Assert.AreEqual("John Doe: Guitar, vocals (1992-2015)", member.ToString());
        }
    }
}
