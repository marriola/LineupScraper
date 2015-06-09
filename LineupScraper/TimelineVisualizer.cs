using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    class TimelineVisualizer
    {
        protected static readonly Color[] palette =
            { Color.Blue, Color.Crimson, Color.Green, Color.Purple, Color.DarkMagenta,
              Color.Teal, Color.Goldenrod, Color.OrangeRed, Color.DodgerBlue,
              Color.Chocolate };

        public static readonly int MAX_ROLES = palette.Length;

        public static void Save(string bandName, Timeline timeline)
        {
        }
    }
}
