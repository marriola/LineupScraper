using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    abstract class TimelineVisualizer
    {
        public static readonly Color[] palette =
            { Color.Blue, Color.Crimson, Color.Green, Color.DarkMagenta,
              Color.Teal, Color.Goldenrod, Color.OrangeRed, Color.DodgerBlue,
              Color.Chocolate, Color.LawnGreen, Color.Cyan, Color.DeepPink,
              Color.LightBlue, Color.LightCoral, Color.Khaki, Color.Orchid };

        public static readonly int MAX_ROLES = palette.Length;

        protected string bandName;
        protected Timeline timeline;

        public TimelineVisualizer(string bandName, Timeline timeline)
        {
            this.bandName = bandName;
            this.timeline = timeline;
        }
        
        public abstract void Save();
    }
}
