using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    class DFARoleParser
    {
        private string roleString;

        public List<RoleInterval> sections
        {
            get;
            private set;
        }

        public DFARoleParser(string roleString)
        {
            this.roleString = roleString;
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
            ENDYEAR,
            NEWYEAR
        };

        private static readonly State[,] transitionTable =
        {
/*               space            ,                (                )                -              other */
/* START     */ {State.START,     State.START,     State.START,     State.START,     State.ROLE,    State.ROLE},
/* ROLE      */ {State.ROLE,      State.NEWROLE,   State.STARTYEAR, State.ROLE,      State.ROLE,    State.ROLE},
/* NEWROLE   */ {State.NEWROLE,   State.NEWROLE,   State.NEWROLE,   State.NEWROLE,   State.NEWROLE, State.ROLE},
/* STARTYEAR */ {State.STARTYEAR, State.NEWYEAR,   State.STARTYEAR, State.START,     State.ENDYEAR, State.STARTYEAR},
/* ENDYEAR   */ {State.ENDYEAR,   State.NEWYEAR,   State.ENDYEAR,   State.START,     State.ENDYEAR, State.ENDYEAR},
/* NEWYEAR   */ {State.NEWYEAR,   State.NEWYEAR,   State.NEWYEAR,   State.START,     State.NEWYEAR, State.STARTYEAR}
        };

        private State AdvanceParser(State state, char c)
        {
            int transitionIndex = alphabet.Contains(c) ? alphabet.IndexOf(c) : alphabet.Length;
            return transitionTable[(int)state, transitionIndex];
        }

        private string TrimToken(string token, Func<char, bool> predicate)
        {
            while (token.Length > 0 && predicate(token.Last()))
            {
                token = token.Substring(0, token.Length - 1);
            }
            return token;
        }

        public List<RoleInterval> Parse()
        {
            List<RoleInterval> rolePairs = new List<RoleInterval>();
            List<string> roleList = new List<string>();
            List<YearInterval> yearIntervalList = new List<YearInterval>();
            string currentToken = "" + roleString[0];
            State state = AdvanceParser(State.START, roleString[0]);
            State lastState = State.START;

            for (int i = 1; i < roleString.Length; i++)
            {
                char c = roleString[i];
                lastState = state;
                state = AdvanceParser(state, c);
                // This check keeps us from including delimiters in between tokens.
                if (state != lastState || (state != State.START && state != State.NEWROLE && state != State.NEWYEAR))
                {
                    currentToken += c;
                }

                // ROLE -> STARTYEAR
                // Make sure that the stuff in parentheses is actually a year
                // and not an instrument specifier, e.g. guitar (lead) or
                // vocals (backing).
                if (i < roleString.Length - 1 && state == State.STARTYEAR && Char.IsLetter(roleString[i + 1]))
                {
                    state = State.ROLE;
                }

                // ROLE -> NEWROLE
                // Save this token as the current role, minus any non-alphanumeric
                // stuff at the end.
                else if (state != State.ROLE && lastState == State.ROLE)
                {
                    currentToken = currentToken.Substring(0, currentToken.Length - 1).Trim();
                    if (currentToken.Length > 0)
                    {
                        roleList.Add(currentToken);
                        currentToken = "";
                    }
                }

                // STARTYEAR or ENDYEAR -> NEWYEAR
                // Save the year token before starting the next.
                else if (state == State.NEWYEAR)
                {
                    currentToken = TrimToken(currentToken, x => !Char.IsLetterOrDigit(x));
                    if (lastState == State.STARTYEAR)
                    {
                        yearIntervalList.Add(new YearInterval(currentToken));
                    }
                    else if (lastState == State.ENDYEAR)
                    {
                        yearIntervalList.Last().SetEndYear(currentToken);
                    }
                    currentToken = "";
                }

                // STARTYEAR -> ENDYEAR
                // Save this token as the start year.
                else if (state == State.ENDYEAR && lastState == State.STARTYEAR)
                {
                    // chop off non-digits from the start year if present
                    currentToken = TrimToken(currentToken, x => !(x == '?' || Char.IsLetterOrDigit(x)));
                    yearIntervalList.Add(new YearInterval(currentToken));
                    currentToken = "";
                }

                // STARTYEAR or ENDYEAR -> START
                // Done parsing this role-year pair.
                else if (state == State.START && (lastState == State.STARTYEAR || lastState == State.ENDYEAR))
                {
                    if (lastState == State.STARTYEAR)
                    {
                        // chop off non-digits from the end year if present
                        currentToken = TrimToken(currentToken, x => !Char.IsLetterOrDigit(x));
                        yearIntervalList.Add(new YearInterval(currentToken));
                    }
                    else if (lastState == State.ENDYEAR)
                    {
                        // chop off non-digits from the end year if present
                        currentToken = TrimToken(currentToken, x => !Char.IsLetterOrDigit(x));
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
