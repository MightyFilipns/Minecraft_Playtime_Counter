using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Playtime_Counter
{

    /// <summary>
    /// the core class of the program. Handles all the counting, decompressing and other stuff
    /// </summary>
    public static class logic
    {
        //clean up the variables
        public static readonly TimeSpan tsday = TimeSpan.FromHours(24);
        static List<string> finalpaths = new List<string>();
        public static TimeSpan total = new TimeSpan();
        public static List<timedata> data = new List<timedata>();
        public static Stopwatch sw = new Stopwatch();
        
        public static string dirname;
        public static int scanned = 0;
        public static int totalcount = 0;
        public static string longestfile;
        public static int longestlenght;
        public static int folderi = 0;
        //the most important function in the whole program
        public static void startscan()
        {
            sw.Reset();
            sw.Start();
            data = new List<timedata>();
            total = new TimeSpan();
            scanned = 0;
            totalcount = 0;
            string str = Path.GetTempPath() + "\\minecraftlogsdecompressed\\";
            if (!Directory.Exists(str))
            {
                Directory.CreateDirectory(str);
            }
            unpack();
            //this loop decompresses all the files into a temp folder
            //first filtering all out file thta dont start with "20" and dont have the ".gz" extension
            for (int i = 0; i < finalpaths.Count; i++)
            {
                string dirname = finalpaths[i];
                DirectoryInfo di = new DirectoryInfo(dirname);
                folderi = i;
                List<FileInfo> files = di.GetFiles().Where(x => x.Extension == ".gz" && x.Name.StartsWith("20")).ToList();
                
                int loop = 1;
                files.ForEach(x =>
                {
                    DecompressFile(x.FullName, str + "/" + x.Name.Replace(".gz", "").Trim() + "+" + i + 1);
                    logic.dirname = dirname;
                    MainWindow.updatestatus($"Decompressing {loop}/{files.Count} ({((float)loop / (float)files.Count) * 100}%) --- ({dirname.Split('\\').ElementAt(new Index(3, true))}) {i + 1}/{finalpaths.Count}");
                    loop++;
                });
            }
            
            List<FileInfo> logs = new DirectoryInfo(str).GetFiles().ToList();
            totalcount = logs.Count;
            //this loop calls the function that parses the files and adds the data to the list
            logs.ForEach(y =>
            {
                analyzefile(File.ReadAllLines(y.FullName).ToList(), y.Name.Replace(".log", "").Trim());
            });
 
            data.ForEach(x => total += x.timeplayed);
            MainWindow.updatestatus("Done");
            sw.Stop();
            MainWindow.updateUI();
            Directory.Delete(str, true);
        }
        /// <summary>
        /// this function loops through all added folders added in the filechooser window
        /// then if it is modded it loops through each folder in sied thta folder
        /// and addeds "//logs//" to the path and adding it to a finalpath
        /// if it is not modded it just appends "//logs//" to the path and adds it to a finalpath
        /// all log folder location are check if theya are valid
        /// TODO: alert the user if the log folder is not valid
        /// </summary>
        static void unpack()
        {
            finalpaths.Clear();
            foreach (logfolderstr item in MainWindow.ls)
            {
                if (item.modded)
                {
                    string[] newd = Directory.GetDirectories(item.location);
                    for (int i = 0; i < newd.Length; i++)
                    {
                        add(newd[i] + "\\logs\\");
                    }
                }
                else
                {
                    add(item.location + "\\logs\\");
                }
            }
            void add(string input)
            {
                if (Directory.Exists(input))
                {
                    finalpaths.Add(input);
                }
            }
        }
        /// <summary>
        /// Analyzes the file and returns the time played.
        /// </summary>
        /// 
        /// <param name="lines"></param>
        /// <param name="name"></param>
        public static void analyzefile(List<string> lines, string name)
        {
            scanned++;
            MainWindow.updatestatus($"Scanning {scanned}/{totalcount} ({((float)scanned / (float)totalcount) * 100}%) --- ({dirname.Split('\\').ElementAt(new Index(3, true))}) {folderi + 1}/{finalpaths.Count}");
            List<string> split = name.Split('-').ToList();
            DateOnly startdate;
            if (split.Count >= 3)
            {
                startdate = new DateOnly(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
            }
            else
            {
                return;
            }
            if (lines.Count <= 1)
            {
                return;
            }
            //Two time date can be found in minecraft log files 
            //The long format --- example [24apr2069 21:51:16.583]
            //The short format --- example [42:64:29] 
            //I dont know why there are two.
            //Probably cause of the fact thta mod loader like forge and fabric possibly log this diffrent fomr vannila but ive seen forge log both way.

            //long format / date 
            if (lines[0].Length >= 24 && lines[0][0] == '[' && lines[0][23] == ']' && lines[0][9] != ']')
            {
                //first and last line that have dates
                string first = lines.First();
                string last = lines.FindLast(x => x.Length >= 24 && x[0] == '[' && x[23] == ']');
                //getting the first and last date
                DateOnly stdate = DateOnly.Parse(first.Substring(1, 9));
                DateOnly lastdate = DateOnly.Parse(last.Substring(1, 9));
                int days = lastdate.DayNumber - stdate.DayNumber;
                //geting the first and last time
                TimeSpan sttime = TimeSpan.Parse(first.Substring(11, 12));
                TimeSpan lasttime = TimeSpan.Parse(last.Substring(11, 12));
                //checking if dates are the same
                //if so, calculating the Time diffrence
                //if not calculating the time diffrence but taking into account days 
                //
                //Comment added later:
                //This if is not nessecary because the game makes sure that a single log file does not span across multipe days, so for each day it makes a new file regardless.
                //for now im going to leave this if statment but probably going to remove it in the future.
                if (days > 0)
                {
                    TimeSpan stday = tsday - sttime;
                    TimeSpan total = stday + lasttime + TimeSpan.FromDays(days);
                    data.Add(new timedata(total, startdate, lastdate));
                }
                else
                {
                    TimeSpan diff = lasttime - sttime;
                    data.Add(new timedata(diff, startdate, lastdate));
                };
            }
            //short format / no date
            // Comment was added later
            // This should be fixed as ive relized that minecraft makes sure logs file dont span across days.
            // 
            if (lines[0].Length >= 10 && lines[0][0] == '[' && lines[0][9] == ']')
            {
                string time1 = lines.First();
                TimeSpan sttime = TimeSpan.Parse(time1.Substring(1, 8));
                int lasth = -1;
                int days = -1;
                int lastindex = -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Length >= 22 && lines[i][0] == '[' && lines[i][9] == ']')
                    {
                        int a = int.Parse(lines[i][1].ToString());
                        //if this condition is ever satifacted it means that a very rare bug happened
                        //The bug is that minecraft wrote twice to the same log file. (im not sure if it overwrites or it just inserts text)
                        //this should be handled by countinuing to loop through the file to see if the a>=lasth condition is met.
                        //if it is we prtend nothing happened and just continue
                        //if it is not we alert the user and and dont add any time. (stll not a proper fix)
                        if (a < lasth)
                        {
                            days++;
                        }
                        lastindex = i;
                        lasth = a;
                    }
                }
                TimeSpan lasttime = TimeSpan.Parse(lines[lastindex].Substring(1, 8));
                if (days < 0)
                {
                    data.Add(new timedata(lasttime - sttime, startdate, startdate));
                }
                //this else clause will only be executed if the above afformentioned bug is encounetered.
                //Message box should be displayed to the user.
                //
                else
                {
                    showerror("A invalid log file had been encountered. This was probably caused by a rare bug. the log file is "+name, MainWindow.ins);
                    //data.Add(new timedata(tsday - sttime + lasttime + TimeSpan.FromDays(Math.Clamp(days, 0, int.MaxValue)), startdate, startdate.AddDays(days + 2)) { fname = name });
                }
            }
            if (longestlenght < lines.Count)
            {
                longestlenght = lines.Count;
                longestfile = name;
            }
        }
        /// <summary>
        /// Decompresses the .gz file and stores the decompressed file in decmoploc.
        /// Probably stolen from stackoverflow.
        /// </summary>
        /// <param name="compfile"></param>
        /// <param name="decmoploc"></param>
        private static void DecompressFile(string compfile, string decmoploc)
        {
            using FileStream compressedFileStream = File.Open(compfile, FileMode.Open);
            using FileStream outputFileStream = File.Create(decmoploc);
            using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
            decompressor.CopyTo(outputFileStream);
        }

        //This struct is used to store the time data of a log file.
        //it should be modfied to only have one DateOnly.
        public struct timedata
        {
            public timedata(TimeSpan time, DateOnly _startdate, DateOnly _enddate)
            {
                timeplayed = time;
                startdate = _startdate;
                enddate = _enddate;
            }
            public TimeSpan timeplayed;
            public DateOnly startdate;
            public DateOnly enddate;
        }
        /// <summary>
        /// A simple message dialog that displays an error to the user.
        /// </summary>
        /// <param name="str">The message</param>
        /// <param name="w">The owner of the error dialog</param>
        public static async void showerror(string str,Window w)
        {
            await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams() 
            {
                ContentMessage = str,
                ContentHeader = "Error Encountered",
                ContentTitle = "Error"
            }).ShowDialog(w);
        }

    }
}
