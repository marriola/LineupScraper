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

            // Section 0: role 0
            Assert.AreEqual(member.sections[0].roles[0], "Guitar");
            // Section 0: role 1
            Assert.AreEqual(member.sections[0].roles[1], "vocals");
            // Section 0: range 0: start year
            Assert.AreEqual(member.sections[0].years[0].startYear, 1992);
            // Section 0: range 0: end year
            Assert.AreEqual(member.sections[0].years[0].endYear, DateTime.Today.Year);
        }

        [TestMethod]
        public void TestMultipleYearRanges()
        {
            // A year range ending in "present" is just replaced with the
            // current year, since for the purposes of generating a lineup
            // chart it doesn't really matter if the member stopped that year
            // or not. BandMember.ToString's only purpose is for testing that
            // the information was parsed correctly.
            BandMember member = new BandMember("Jane Doe", "Bass (1992-1995, 2003-present), drums (1995-2003)");

            // Section 0: role 0
            Assert.AreEqual(member.sections[0].roles[0], "Bass");
            // Section 0: range 0: start year
            Assert.AreEqual(member.sections[0].years[0].startYear, 1992);
            // Section 0: range 0: end year
            Assert.AreEqual(member.sections[0].years[0].endYear, 1995);
            // Section 0: range 1: start year
            Assert.AreEqual(member.sections[0].years[1].startYear, 2003);
            // Section 0: range 1: end year
            Assert.AreEqual(member.sections[0].years[1].endYear, DateTime.Today.Year);

            // Section 1: role 0
            Assert.AreEqual(member.sections[1].roles[0], "drums");
            // Section 1: range 0: start year
            Assert.AreEqual(member.sections[1].years[0].startYear, 1995);
            // Section 1: range 0: end year
            Assert.AreEqual(member.sections[1].years[0].endYear, 2003);
        }

        [TestMethod]
        public void TestRoleWithParens()
        {
            BandMember member = new BandMember("James Hetfield", "Guitar (rhythm) (1981-present)");
            Assert.AreEqual("Guitar (rhythm)", member.sections[0].roles[0]);
        }
    }
}
