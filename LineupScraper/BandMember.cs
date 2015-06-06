﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
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
            private set;
        }

        public void SetStartYear(string year)
        {
            startYear = year.Equals("?") ? int.MinValue : Convert.ToInt32(year);
        }

        public void SetEndYear(string year)
        {
            endYear = year.Equals("?") ? int.MaxValue : Convert.ToInt32(year);
        }

        public string ToString()
        {
            string startYearString = startYear == int.MinValue ? "?" : Convert.ToString(startYear);
            string endYearString = endYear == int.MaxValue ? "?" : Convert.ToString(endYear);
            return startYearString + "-" + endYearString;
        }

        public YearInterval(string startYear, string endYear = "?")
        {
            SetStartYear(startYear);
            SetEndYear(endYear);
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

        public RoleInterval(string[] roles, YearInterval[] years)
        {
            this.roles = roles;
            this.years = years;
        }
    }

    public class BandMember
    {
        public string name
        {
            get;
            private set;
        }

        public List<RoleInterval> roles
        {
            get;
            private set;
        }

        public BandMember(string name, string roleString)
        {
            this.name = name;
            this.roles = ParseRoles(roleString);
        }

        public string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name + ": ");
            foreach (RoleInterval rolePair in roles)
            {
                builder.Append(string.Join(", ", rolePair.roles));
                builder.Append(" (");
                builder.Append(string.Join(", ", Array.ConvertAll(rolePair.years, x => x.ToString())));
                builder.Append(")");
            }
            return builder.ToString();
        }

        /**
         * Role parser
         * 
         * Parses roles in this format:
         * role1, role2, ..., roleN (startYear[-endYear])
         */

        private const string alphabet = " ,()-";

        private enum State
        {
            START,
            ROLE,
            NEWROLE,
            STARTYEAR,
            ENDYEAR
        };

        private static readonly State[,] transitionTable =
        {
/*               space            ,                (                )                -              other */
/* START     */ {State.START,     State.START,     State.START,     State.START,     State.ROLE,    State.ROLE},
/* ROLE      */ {State.ROLE,      State.NEWROLE,   State.STARTYEAR, State.ROLE,      State.ROLE,    State.ROLE},
/* NEWROLE   */ {State.NEWROLE,   State.NEWROLE,   State.NEWROLE,   State.NEWROLE,   State.NEWROLE, State.ROLE},
/* STARTYEAR */ {State.STARTYEAR, State.STARTYEAR, State.STARTYEAR, State.STARTYEAR, State.ENDYEAR, State.STARTYEAR},
/* ENDYEAR   */ {State.ENDYEAR,   State.STARTYEAR, State.ENDYEAR,   State.START,     State.ENDYEAR, State.ENDYEAR}
        };

        private static State AdvanceParser(State state, char c)
        {
            int transitionIndex = alphabet.Contains(c) ? alphabet.IndexOf(c) : alphabet.Length;
            return transitionTable[(int)state, transitionIndex];
        }

        private static List<RoleInterval> ParseRoles(string roleString)
        {
            List<RoleInterval> rolePairs = new List<RoleInterval>();
            List<string> roleList = new List<string>();
            List<YearInterval> yearIntervalList = new List<YearInterval>();
            string currentToken = "" + roleString[0];
            State state = AdvanceParser(State.START, roleString[0]);
            State lastState;

            foreach (char c in roleString.Substring(1))
            {
                currentToken += c;
                lastState = state;
                state = AdvanceParser(state, c);

                // ROLE -> NEWROLE
                // Save this token as the current role, minus any non-alphanumeric
                // stuff at the end.
                if (state != State.ROLE && lastState == State.ROLE)
                {
                    while (!Char.IsLetterOrDigit(currentToken.Last()))
                    {
                        currentToken = currentToken.Substring(0, currentToken.Length - 1);
                    }

                    if (currentToken.Length > 0)
                    {
                        roleList.Add(currentToken);
                        currentToken = "";
                    }
                }

                // Make sure the next role token doesn't start with a space.
                else if (state == State.NEWROLE)
                {
                    currentToken = "";
                }

                // STARTYEAR -> ENDYEAR
                // Save this token as the start year.
                else if (state == State.ENDYEAR && lastState == State.STARTYEAR)
                {
                    // chop off non-digits from the start year if present
                    if (!Char.IsDigit(currentToken.Last()))
                    {
                        currentToken = currentToken.Substring(0, currentToken.Length - 1);
                    }
                    yearIntervalList.Add(new YearInterval(currentToken));
                    currentToken = "";
                }

                // STARTYEAR or ENDYEAR -> START
                // Done parsing this role-year pair.
                else if (state == State.START && (lastState == State.STARTYEAR || lastState == State.ENDYEAR))
                {
                    if (lastState == State.ENDYEAR)
                    {
                        // chop off non-digits from the end year if present
                        if (!Char.IsDigit(currentToken.Last()))
                        {
                            currentToken = currentToken.Substring(0, currentToken.Length - 1);
                        }
                        yearIntervalList.Last().SetEndYear(currentToken);
                    }
                    currentToken = "";

                    rolePairs.Add(new RoleInterval(roleList.ToArray(), yearIntervalList.ToArray()));
                    roleList = new List<string>();
                    yearIntervalList = new List<YearInterval>();
                }
            }

            return rolePairs;
        }
    }
}