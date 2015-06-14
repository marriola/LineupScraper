using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    abstract class TimelineVisualizer
    {
        private static readonly Color[] palette =
            { Color.Blue, Color.Crimson, Color.Green, Color.DarkMagenta,
              Color.Teal, Color.Goldenrod, Color.OrangeRed, Color.DodgerBlue,
              Color.Chocolate, Color.LawnGreen, Color.Cyan, Color.DeepPink,
              Color.LightBlue, Color.LightCoral, Color.Khaki, Color.Orchid };

        protected readonly SolidBrush[] solidPalette;
        protected readonly HatchBrush[] hatchPalette;

        public static readonly int MAX_ROLES = palette.Length;

        protected string bandName;
        protected Timeline timeline;

        public TimelineVisualizer(string bandName, Timeline timeline)
        {
            this.bandName = bandName;
            this.timeline = timeline;

            solidPalette = new SolidBrush[palette.Length];
            hatchPalette = new HatchBrush[palette.Length];
            for (int i = 0; i < palette.Length; i++)
            {
                solidPalette[i] = new SolidBrush(palette[i]);
                hatchPalette[i] = new HatchBrush(HatchStyle.WideDownwardDiagonal, palette[i], Color.Transparent);
            }
        }

        ~TimelineVisualizer()
        {
            for (int i = 0; i < palette.Length; i++)
            {
                solidPalette[i].Dispose();
                hatchPalette[i].Dispose();
            }
        }

        public abstract void Save();
    }
}
