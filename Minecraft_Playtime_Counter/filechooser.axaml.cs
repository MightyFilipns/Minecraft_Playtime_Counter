using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Minecraft_Playtime_Counter
{
    public partial class filechooser : Window
    {
        int columew
        {
            get
            {
                return (int)(ClientSize.Width * 0.9f);
            }
        }
        public static List<logfolderstr> dirs = new List<logfolderstr>();
        Button adddirbtn;
        StackPanel stp1;
        StackPanel modpanel;
        StackPanel rmpnl;
        Grid grdl;
        public filechooser()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            adddirbtn = this.Find<Button>("addbtn");
            adddirbtn.Click += Adddirbtn_Click;
            stp1 = this.Find<StackPanel>("stckp");
            grdl = this.Find<Grid>("grd");
            modpanel = this.Find<StackPanel>("modpnl");
            rmpnl = this.Find<StackPanel>("removepnl");
            grdl.ColumnDefinitions[0] = new ColumnDefinition(columew, GridUnitType.Pixel);
            //to properly resize colume weh the windows is resized
            ClientSizeProperty.Changed.AddClassHandler<filechooser>((a, b) =>
            {
                grdl.ColumnDefinitions[0].Width = new GridLength(columew, GridUnitType.Pixel);
            });
            int i = 0;
            dirs.ForEach(a => {
                adddir(a.location, a.modded, i, false);
                i++;
            });
        }

        private async void Adddirbtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Button btn = sender as Button;
            OpenFolderDialog a = new OpenFolderDialog();
            a.Title = "Set log folder";
            string res = await a.ShowAsync(this);
            if (dirs.Any(a => { return res.StartsWith(a.location) || a.location.StartsWith(res); }))
            {
                var msgb = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams()
                {
                    ContentHeader = "Error Encountered",
                    ContentMessage = "Path already added. Please Try Again",
                    ContentTitle = "Error",
                    Width = 300
                });
                await msgb.ShowDialog(this);
                return;
            }
            if (res == "" || !Directory.Exists(res))
            {
                var msgb = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams()
                {
                    ContentHeader = "Error Encountered",
                    ContentMessage = "Invalid Folder Path. Please Try Again",
                    ContentTitle = "Error",
                    Width = 300
                });
                await msgb.ShowDialog(this);
                return;
            }
            adddir(res, false, dirs.Count, true);

        }
        void adddir(string res, bool mod, int ii, bool newe)
        {
            Label btn1 = new Label();
            btn1.Background = dirs.Count % 2 == 0 ? Brush.Parse("Gray") : Brush.Parse("DarkGray");
            btn1.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
            btn1.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            btn1.Name = "aaaa";
            btn1.Content = res;
            btn1.FontSize = 21.5;
            ToggleSwitch tw = new ToggleSwitch();
            tw.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            tw.IsChecked = mod;
            stp1.Children.Add(btn1);
            modpanel.Children.Add(tw);
            logfolderstr lfs = new logfolderstr(res, false);
            if (newe)
            {
                dirs.Add(lfs);
            }
            //int len = dirs.Count - 1;
            tw.AddHandler(Avalonia.Controls.Primitives.ToggleButton.CheckedEvent, (a, b) =>
            {
                dirs[ii].modded = (bool)tw.IsChecked;
            });
            Button rmvbtn = new Button();
            rmvbtn.Content = "Remove";
            rmvbtn.FontSize = 16;
            rmpnl.Children.Add(rmvbtn);
            var asd = dirs[ii];
            rmvbtn.Click += (a, b) => {
                dirs.Remove(asd);
                stp1.Children.Remove(btn1);
                modpanel.Children.Remove(tw);
                rmpnl.Children.Remove(rmvbtn);
            };
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
    public class logfolderstr
    {
        public logfolderstr(string location, bool modded)
        {
            this.location = location;
            this.modded = modded;
        }
        public string location { get; set; }
        public bool modded { get; set; }
    }
}
