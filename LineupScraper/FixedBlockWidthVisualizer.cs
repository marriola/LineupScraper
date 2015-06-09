﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineupScraper
{
    class FixedBlockWidthVisualizer : TimelineVisualizer
    {
        private const int PADDING = 5;
        private const int ROW_WIDTH = TimelineRow.DEFAULT_HEIGHT;

        public FixedBlockWidthVisualizer(string bandName, Timeline timeline)
            : base(bandName, timeline)
        {
        }

        private Bitmap DrawLegend()
        {
            Bitmap legend = new Bitmap(0, 0);
            return legend;
        }

        private static void drawBlock(int rowHeight, int roles, Graphics g, int x, int y)
        {
            if (roles == 0)
            {
                return;
            }

            // Build up a list of the colors present in this block.
            List<Color> roleColors = new List<Color>();
            for (int i = 1; i < MAX_ROLES; i++)
            {
                if ((roles & (int)Math.Pow(2, i)) != 0)
                {
                    roleColors.Add(palette[i - 1]);
                }
            }

            int stripHeight = rowHeight / roleColors.Count;
            foreach (Color color in roleColors)
            {
                Brush brush;
                if ((roles & Timeline.INDETERMINATE_END_YEAR) != 0 ||
                    (roles & Timeline.INDETERMINATE_START_YEAR) != 0)
                {
                    brush = new HatchBrush(HatchStyle.WideDownwardDiagonal, color, Color.LightGray);
                }
                else
                {
                    brush = new SolidBrush(color);
                }

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
            Font labelFont = new Font("Arial", 11);
            Brush labelBrush = new SolidBrush(Color.Black);
            Brush shadeBrush = new SolidBrush(Color.Gainsboro);
            Pen yearGridLight = new Pen(new SolidBrush(Color.LightGray));
            Pen yearGridDark = new Pen(new SolidBrush(Color.Gray));

            // Compute longest band member name label.
            Bitmap throwawayBitmap = new Bitmap(1, 1);
            Graphics throwawayGraphics = Graphics.FromImage(throwawayBitmap);
            int labelsWidth = PADDING + timeline.band.Aggregate(0, (max, member) =>
                Math.Max(max, (int)throwawayGraphics.MeasureString(member.name, labelFont).Width));
            throwawayGraphics.Dispose();
            throwawayBitmap.Dispose();

            int rowHeightSum = timeline.chart.Aggregate(0, (accumulator, row) => accumulator + row.height + TimelineRow.ROW_GAP);
            int chartWidth = labelsWidth + (PADDING * 3) + (timeline.endYear - timeline.startYear + 1) * ROW_WIDTH;
            Bitmap chartBitmap = new Bitmap(chartWidth, rowHeightSum + 20);
            Graphics g = Graphics.FromImage(chartBitmap);

            g.Clear(Color.White);

            // Draw the chart.
            int y = 0;
            bool shadeRow = true;
            foreach (TimelineRow row in timeline.chart)
            {
                int rowHeight = row.height + TimelineRow.ROW_GAP;

                if (shadeRow)
                {
                    g.FillRectangle(shadeBrush, 0, y, chartWidth, rowHeight);
                }

                g.DrawString(row.name, labelFont, labelBrush, PADDING, (float)Math.Ceiling(y + (float)PADDING / 2));
                for (int col = 0; col < row.years.Length; col++)
                {
                    int blockX = labelsWidth + col * TimelineRow.DEFAULT_HEIGHT;
                    g.DrawLine(col % 5 == 0 ? yearGridDark : yearGridLight, blockX, y, blockX, y + rowHeight);
                    drawBlock(row.height, row.years[col], g, blockX, y + 8);
                }

                y += rowHeight;
                shadeRow = !shadeRow;
            }

            // Draw the year labels
            int x = labelsWidth;
            for (int year = timeline.startYear; year <= timeline.endYear; year += 5)
            {
                string yearString = Convert.ToString(year);
                int width = (int)g.MeasureString(yearString, labelFont).Width;
                g.DrawString(yearString, labelFont, labelBrush, x - width / 2, y);
                x += 5 * TimelineRow.DEFAULT_HEIGHT;
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
