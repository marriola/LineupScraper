using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    class FixedBlockWidthVisualizer : TimelineVisualizer
    {
        private static int ROW_WIDTH = TimelineRow.DEFAULT_HEIGHT;

        public FixedBlockWidthVisualizer(string bandName, Timeline timeline)
            : base(bandName, timeline)
        {
        }

        private static void drawBlock(int rowHeight, int roles, Graphics g, int x, int y)
        {
            if (roles == 0)
            {
                return;
            }

            // Build up a list of the colors present in this block.
            List<SolidBrush> roleColors = new List<SolidBrush>();
            for (int i = 1; i < MAX_ROLES; i++)
            {
                if ((roles & (int)Math.Pow(2, i)) != 0)
                {
                    roleColors.Add(new SolidBrush(palette[i - 1]));
                }
            }

            int stripHeight = rowHeight / roleColors.Count;
            foreach (Brush brush in roleColors)
            {
                g.FillRectangle(brush, new Rectangle(x, y, ROW_WIDTH, stripHeight));
                y += stripHeight;
                brush.Dispose();
            }
        }

        /**
         * Generates a PNG of the timeline chart.
         */
        public override void Save()
        {
            int labelsWidth = timeline.band.Aggregate(0, (max, member) =>
                Math.Max(max, member.name.Length * 8));
            int chartWidth = labelsWidth + 15 + (timeline.endYear - timeline.startYear + 1) * ROW_WIDTH;
            int rowHeightSum = timeline.chart.Aggregate(0, (accumulator, row) => accumulator + row.height + TimelineRow.ROW_GAP);

            Bitmap chartBitmap = new Bitmap(chartWidth, rowHeightSum);
            Graphics g = Graphics.FromImage(chartBitmap);
            Font labelFont = new Font("Arial", 11);
            Brush labelBrush = new SolidBrush(Color.Black);
            Brush shadeBrush = new SolidBrush(Color.Gainsboro);
            Brush yearGrid = new SolidBrush(Color.LightGray);
            Pen yearGridPen = new Pen(yearGrid);

            g.Clear(Color.White);

            int y = 0;
            bool shadeRow = true;
            foreach (TimelineRow row in timeline.chart)
            {
                int rowHeight = row.height + TimelineRow.ROW_GAP;

                if (shadeRow)
                {
                    g.FillRectangle(shadeBrush, 0, y, chartWidth, rowHeight);
                }

                g.DrawString(row.name, labelFont, labelBrush, 5, y + 3);
                for (int col = 0; col < row.years.Length; col++)
                {
                    int blockX = labelsWidth + col * TimelineRow.DEFAULT_HEIGHT;
                    if (col % 5 == 0)
                    {
                        // Draw a guideline every five years
                        g.DrawLine(yearGridPen, blockX, y, blockX, y + rowHeight);
                    }
                    drawBlock(row.height, row.years[col], g, blockX, y + 8);
                }

                y += rowHeight;
                shadeRow = !shadeRow;
            }

            chartBitmap.Save(bandName + ".png", ImageFormat.Png);
            chartBitmap.Dispose();
            g.Dispose();
            labelFont.Dispose();
            labelBrush.Dispose();
            shadeBrush.Dispose();
        }
    }
}
