﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraperLibrary
{
    /**
     * A time interval for a band member's role. Unknown or missing start and
     * end years are represented as int.MinValue and int.MaxValue respectively.
     */
    public class YearInterval
    {
        public int startYear
        {
            get;
            private set;
        }

        public int endYear
        {
            get;
            set;
        }

        public void SetStartYear(string year)
        {
            startYear = year.Equals("?") ? int.MinValue : Convert.ToInt32(year);
        }

        public void SetEndYear(string year)
        {
            if (year.Length == 0)
            {
                endYear = startYear;
            }
            else if (year.Equals("?"))
            {
                endYear = int.MaxValue;
            }
            else if (year.ToLower().Equals("present"))
            {
                endYear = DateTime.Today.Year;
            }
            else
            {
                endYear = Convert.ToInt32(year);
            }
        }

        public override string ToString()
        {
            string startYearString = startYear == int.MinValue ? "?" : Convert.ToString(startYear);
            string endYearString = endYear == int.MaxValue ? "?" : Convert.ToString(endYear);
            return startYearString + "-" + endYearString;
        }

        public YearInterval(string startYear)
        {
            SetStartYear(startYear);
            this.endYear = this.startYear;
        }

        public YearInterval(int startYear, int endYear)
        {
            this.startYear = startYear;
            this.endYear = endYear;
        }
    }

    /**
     * A band role associated with a start and end year. E.g., drums from 1972 to 2014, or bass and vocals from 1980 to 1989.
     */
    public class RoleInterval
    {
        public string[] roles
        {
            get;
            private set;
        }

        public YearInterval[] years
        {
            get;
            private set;
        }

        public RoleInterval(string[] roles, YearInterval[] years, int startYear, int endYear)
        {
            this.roles = roles;
            if (years.Length == 0)
            {
                this.years = new YearInterval[1];
                this.years[0] = new YearInterval(startYear, endYear);
            }
            else
            {
                this.years = years;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Join(", ", roles));
            builder.Append(" (");
            builder.Append(string.Join(", ", Array.ConvertAll(years, x => x.ToString())));
            builder.Append(")");
            return builder.ToString();
        }
    }

    public class BandMember
    {
        public string name
        {
            get;
            private set;
        }

        public List<RoleInterval> sections
        {
            get;
            private set;
        }

        public BandMember(string name, string roleString, int startYear, int endYear)
        {
            this.name = name;
            this.sections = new DFARoleParser(roleString, startYear, endYear).Parse();
        }

        public override string ToString()
        {
            return name + ": " + string.Join(", ", Array.ConvertAll(sections.ToArray(), x => x.ToString()));
        }
    }
}