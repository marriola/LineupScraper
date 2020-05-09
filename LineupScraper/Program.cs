using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LineupScraperLibrary;

namespace LineupScraper
{
    class Program
    {
        static string PickBand(List<Tuple<string, string>> bandList)
        {
            for (int i = 0; i < bandList.Count; i++)
            {
                Console.WriteLine(string.Format("{0}: {1}", i + 1, bandList[i].Item1));
            }

            int choice = 0;
            do
            {
                Console.Write("Please select a band: ");
                try
                {
                    choice = Convert.ToInt32(Console.ReadLine()) - 1;
                    return bandList[choice].Item2;
                }
                catch (Exception)
                {
                    continue;
                }
            } while (choice < 0 || choice >= bandList.Count);

            return string.Empty;
        }

        [STAThread]
        static void Main(string[] args)
        {
            string bandName;
            if (args.Length == 0)
            {
                Console.Write("Band name? ");
                bandName = Console.ReadLine();
            }
            else
            {
                bandName = args[0];
            }

            var bandList = LineupScraperLibrary.LineupScraper.SearchBands(bandName);
            string bandPageUrl = string.Empty;

            if (bandList.Count == 0)
            {
                Console.WriteLine("Not found!");
                return;
            }
            else if (bandList.Count == 1)
            {
                bandPageUrl = bandList[0].Item2;
            }
            else
            {
                bandPageUrl = PickBand(bandList);
            }

            if (!string.IsNullOrWhiteSpace(bandPageUrl))
            {
                TimelineVisualizer timeline = null;

                try
                {
                    timeline = LineupScraperLibrary.LineupScraper.CreateVisualization(bandPageUrl);
                }
                catch (PageLoadException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
#if !DEBUG
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
#endif

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = timeline.BandName + ".png",
                    Filter = "PNG image (*.png)|*.png|JPEG image (*.jpg)|*.jpg|GIF image (*.gif)|*.gif|Bitmap (*.bmp)|*.bmp"
                };

                saveFileDialog.FileOk += (sender, e) =>
                {
                    if (saveFileDialog.FileName != string.Empty)
                    {
                        timeline.Save(saveFileDialog.FileName);
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }
                };

                saveFileDialog.ShowDialog();
            }
        }
    }
}
