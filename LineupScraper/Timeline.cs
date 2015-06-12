using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    /**
     * A timeline row is represented as a name and an integer for each year
     * the band has existed. Each role they play has an associated integer,
     * a power of two, and each one the band member played that year is
     * summed into that slot in the array.
     * 
     * If either year is indeterminate (i.e. "?" was given for that part of
     * the range), one of the upper bits is set.
     */
    class TimelineRow
    {
        public const int STRIP_HEIGHT = 5;
        public const int DEFAULT_HEIGHT_IN_STRIPS = 2;
        public const int DEFAULT_HEIGHT = DEFAULT_HEIGHT_IN_STRIPS * STRIP_HEIGHT;
        public const int ROW_GAP = 15;

        public string name
        {
            get;
            private set;
        }

        public int[] years
        {
            get;
            private set;
        }

        public int height
        {
            get;
            private set;
        }

        public TimelineRow(BandMember member, int bandStartYear, int bandEndYear, Dictionary<string, int> roles)
        {
            name = member.name;
            HashSet<string> rolesUsed = new HashSet<string>();
            height = DEFAULT_HEIGHT;
            years = new int[bandEndYear - bandStartYear + 1];

            foreach (RoleInterval section in member.sections)
            {
                int roleSum = section.roles.Aggregate(0,
                    (accumulator, role) =>
                    {
                        rolesUsed.Add(role);
                        return accumulator + roles[role];
                    });

                foreach (YearInterval roleYears in section.years)
                {
                    int startYear = roleYears.startYear;
                    int endYear = roleYears.endYear;
                    int roleTag = 0;

                    if (roleYears.startYear == int.MinValue)
                    {
                        startYear = bandStartYear;
                        roleTag = Timeline.INDETERMINATE_START_YEAR;
                    }

                    if (roleYears.endYear == int.MaxValue)
                    {
                        endYear = bandEndYear;
                        roleTag = Timeline.INDETERMINATE_END_YEAR;
                    }

                    // If a role has only a start year, it takes that entire block.
                    // Otheriwse, it only goes up to the end year, but not into it.
                    if (startYear == endYear)
                    {
                        years[startYear - bandStartYear] = roleSum + roleTag;
                    }
                    else
                    {

                        for (int year = startYear; year < endYear; year++)
                        {
                            years[year - bandStartYear] = roleSum + roleTag;
                        }
                    }
                }
            }

            // Increase the row height if we have more roles than will fit in
            // the default row height.
            if (rolesUsed.Count > DEFAULT_HEIGHT_IN_STRIPS)
            {
                height = rolesUsed.Count * STRIP_HEIGHT;
            }
        }
    }

    class Timeline
    {
        public const int INDETERMINATE_START_YEAR = 0x20000000;
        public const int INDETERMINATE_END_YEAR = 0x40000000;

        public int startYear
        {
            get;
            private set;
        }

        public int endYear
        {
            get;
            private set;
        }

        public List<TimelineRow> chart
        {
            get;
            private set;
        }

        public List<BandMember> band
        {
            get;
            private set;
        }

        public Dictionary<string, int> roles
        {
            get;
            private set;
        }

        public Timeline(int startYear, int endYear, List<BandMember> band)
        {
            this.startYear = startYear;
            this.endYear = endYear;
            this.band = band;
            Build();
        }

        /**
         * Returns a dictionary mapping band roles to bit mask values.
         */
        Dictionary<string, int> GetRoles()
        {
            Dictionary<string, int> roles = new Dictionary<string, int>();
            int numRoles = 1;
            foreach (BandMember member in band)
            {
                foreach (RoleInterval section in member.sections)
                {
                    foreach (string role in section.roles)
                    {
                        if (!roles.ContainsKey(role))
                        {
                            if (numRoles == TimelineVisualizer.MAX_ROLES)
                            {
                                throw new Exception("Too many roles!");
                            }
                            roles[role] = (int)Math.Pow(2, numRoles);
                            numRoles++;
                        }
                    }
                }
            }
            return roles;
        }

        /**
         * Builds the timeline chart.
         */
        private void Build()
        {
            roles = GetRoles();
            chart = new List<TimelineRow>();
            foreach (BandMember member in band)
            {
                chart.Add(new TimelineRow(member, startYear, endYear, roles));
            }
        }

    }
}
