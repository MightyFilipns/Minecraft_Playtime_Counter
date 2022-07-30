using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Minecraft_Playtime_Counter
{
    public partial class MainWindow : Window
    {
        public Button flocbtn;
        public static List<logfolderstr> ls;
        public static Label statuslbl;
        static Button startbtn;
        static Label timep;
        static Label longfile;
        static Label numlogfiles;
        static Label timetk;
        public static MainWindow ins;
        static StackPanel mainpnl;
        public MainWindow()
        {
            InitializeComponent();
            flocbtn = this.Find<Button>("locbtn");
            statuslbl = this.Find<Label>("stlbl");
            startbtn = this.Find<Button>("Start");
            timep = this.Find<Label>("lb1");
            longfile = this.Find<Label>("lb2");
            numlogfiles = this.Find<Label>("lb3");
            timetk = this.Find<Label>("lb4");
            mainpnl = this.Find<StackPanel>("st1");
            flocbtn.Click += (sender, args) =>
            {
                openfoldermenu();
            };
            startbtn.Click += (sender, args) =>
            {
                start();
            };
            ls = filechooser.dirs;
            ins = this;
        }
        public static void openfoldermenu()
        {
            filechooser fc = new filechooser();
            fc.ShowDialog(ins);
        }
        public async void start()
        {
            if (ls is null || ls.Count == 0)
            {
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams()
                {
                    ContentMessage = "No Folders selected",
                    ContentTitle = "Error"
                }).ShowDialog(this);
                return;
            }
            Thread th = new Thread(() =>
            {
                logic.startscan();
            });
            th.Name = "Logic Thread";
            th.Start();
        }
        
        public static void updatestatus(string str)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { statuslbl.Content = str; }, Avalonia.Threading.DispatcherPriority.Render);
        }
        
        public static void updateUI()
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                timep.Content = $"Total Time played:{Math.Round(logic.total.TotalHours)}h ({logic.total.Days}d)";
                numlogfiles.Content = "Number of log files:" + logic.totalcount;
                longfile.Content = $"Longest log file:{logic.longestfile}.log {logic.longestlenght} lines";
                timetk.Content = "Time Taken:" + logic.sw.Elapsed.TotalSeconds + "s";
            }, Avalonia.Threading.DispatcherPriority.Render);
        }

    }
}
