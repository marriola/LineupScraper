using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LineupScraperLibrary
{
    class RegexRoleParser
    {
        private string roleString;
        private static readonly Regex rolePairRegex = new Regex("(?<roles>(?<role>[a-zA-z\\s]*?)\\((?<years>(?<start>\\d*|\\?)(?<end>-(\\d*|\\?\\present))?(,\\s*)?)(\\?|\\d.*?)\\)?)(,\\s*)?");

        public RegexRoleParser(string roleString)
        {
            this.roleString = roleString;
        }

        public List<RoleInterval> Parse()
        {
            List<RoleInterval> rolePairs = new List<RoleInterval>();
            //Match rolePairMatch = rolePairRegex.Match(roleString);

            //// Parse role-year pair list
            //while (rolePairMatch.Success)
            //{
            //    Match rolesMatch = rolesRegex.Match(rolePairMatch.Groups["roles"].Value);
            //    string years = rolesMatch.Groups["years"].Value;

            //    // Parse year list
            //    Match yearIntervals = yearsRegex.Match(years);
            //    while (yearIntervals.Success)
            //    {
            //        // Parse years
            //        yearIntervals = yearIntervals.NextMatch();
            //    }

            //    rolePairMatch = rolePairMatch.NextMatch();
            //}
            return rolePairs;
        }
    }
}
