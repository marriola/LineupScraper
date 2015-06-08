using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
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
        private const int STRIP_HEIGHT = 3;
        private const int DEFAULT_HEIGHT_IN_STRIPS = 5;

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
            HashSet<string> rolesUsed = new HashSet<string>();
            height = STRIP_HEIGHT * DEFAULT_HEIGHT_IN_STRIPS;
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

                    for (int year = startYear; year <= endYear; year++)
                    {
                        years[year - bandStartYear] = roleSum + roleTag;
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

        private List<TimelineRow> timelineChart;
        private List<BandMember> band;

        public Timeline(List<BandMember> band)
        {
            this.band = band;
            Build();
        }

        /**
         * Iterates through each band member's roles, returning either the
         * earliest start year (firstOrlast = true) or the latest end year
         * (firstOrLast = false).
         */
        private int FilterYears(bool firstOrLast)
        {
            int year = firstOrLast ? int.MaxValue : int.MinValue;
            foreach (BandMember member in band)
            {
                foreach (RoleInterval section in member.sections)
                {
                    foreach (YearInterval role in section.years)
                    {
                        int compareYear = firstOrLast ? role.startYear : role.endYear;
                        if ((firstOrLast && compareYear < year) ||
                            (!firstOrLast && compareYear > year))
                        {
                            year = compareYear;
                        }
                    }
                }
            }
            return year;
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
                            if (numRoles == 29)
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

        private void Build()
        {
            int startYear = FilterYears(true);
            int endYear = FilterYears(false);
            Dictionary<string, int> roles = GetRoles();
            timelineChart = new List<TimelineRow>();
            foreach (BandMember member in band)
            {
                timelineChart.Add(new TimelineRow(member, startYear, endYear, roles));
            }
        }

        public void Save()
        {
            int rowHeightSum = timelineChart.Aggregate(0, (accumulator, row) => accumulator + row.height);
            Bitmap chartBitmap = new Bitmap(640, rowHeightSum);
        }
    }
}
