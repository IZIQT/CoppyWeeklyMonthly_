using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.IO.Compression;
using System.Xml.Serialization;

namespace CoppyWeeklyMonthly
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ExeptionMessage;

        #region Таймеры и переменные
        public class PathStruct
        {
            public string PathName;
            public string CatalogName;
        }

        private bool CheckParam1 = false;
        private bool CheckParam2 = false;

        private DispatcherTimer timerMonthly = null;

        private int secMonthly;
        private int minMonthly;
        private int hourMonthly;

        private void timerStartMonthly()
        {
            timerMonthly = new DispatcherTimer();
            timerMonthly.Tick += new EventHandler(timerTickMonthly);
            timerMonthly.Interval = new TimeSpan(0, 0, 0, 1);
            timerMonthly.Start();
        }

        private void timerTickMonthly(object sender, EventArgs e)
        {
            secMonthly++;
            if (secMonthly == 60)
            {
                minMonthly++;
                if (minMonthly == 60)
                {
                    hourMonthly++;
                    minMonthly = 0;
                }
                secMonthly = 0;
            }
            TextTimerMonthly.Text = hourMonthly + ":" + minMonthly + ":" + secMonthly;
        }

        private DispatcherTimer timerWeek = null;

        private int secWeek;
        private int minWeek;
        private int hourWeek;

        private void timerStartWeek()
        {
            timerWeek = new DispatcherTimer();
            timerWeek.Tick += new EventHandler(timerTickWeek);
            timerWeek.Interval = new TimeSpan(0, 0, 0, 1);
            timerWeek.Start();
        }

        private void timerTickWeek(object sender, EventArgs e)
        {
            secWeek++;
            if (secWeek == 60)
            {
                minWeek++;
                if (minWeek == 60)
                {
                    hourWeek++;
                    minWeek = 0;
                }
                secWeek = 0;
            }
            TextTimerWeek.Text = hourWeek + ":" + minWeek + ":" + secWeek;
        }

        private DateTime CurrentDate;

        public SettingWindowViewModel WinSetting { get; set; }

        #endregion

        #region Конструктор
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentDate = DateTime.Now;
                if (WeeklyRButton.IsChecked.Value)//недельное копирование
                {
                    ButtonStartCoppy.IsEnabled = false;
                    CheckParam1 = true;
                    timerStartWeek();
                    WeeklyCoppyCatalog();

                }
                if (MonthlyRButton.IsChecked.Value)//месячное копирование
                {
                    ButtonStartCoppy.IsEnabled = false;
                    CheckParam1 = true;
                    CheckParam2 = true;
                    ScaleVisibylity.Visibility = Visibility.Visible;
                    timerStartWeek();
                    timerStartMonthly();
                    MonthlyCoppyCatalog();
                    WeeklyCoppyCatalog();
                }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Button_Click\n" + exp.ToString() + "\n\n";
            }
        }

        #region Копирование недельное
        private async void WeeklyCoppyCatalog()
        {
            try
            {
                List<PathStruct> PathCollection = new List<PathStruct>();
                using (FileStream fs = new FileStream("data.xml", FileMode.Open))
                {
                    XmlSerializer xmlser = new XmlSerializer(typeof(SettingWindowViewModel));

                    try
                    {
                        WinSetting = (SettingWindowViewModel)xmlser.Deserialize(fs);
                        foreach (SettingCollectionModel item in WinSetting.SerializCollectionFolder.Where(x => x.ParamWM == "Weekly"))
                        {
                            PathCollection.Add(new PathStruct() { CatalogName = item.LocationFolder, PathName = item.FolderName });
                        }
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show("Ошибка!\n\n" + exp, "Ошибка!");
                    }
                }

                PowerTextWeek.Text = "0/" + PathCollection.Count();
                double PowerLavel = (double)100 / (double)PathCollection.Count();
                int CounterMonthly = 0;
                foreach (PathStruct CopyPath in PathCollection)
                {
                    await Task.Run(() => CoppyCompressionCatalog(CopyPath.CatalogName, CopyPath.PathName));
                    CounterMonthly++;
                    PowerTextWeek.Text = CounterMonthly + "/" + PathCollection.Count();
                    PowerScaleWeek.Value = PowerScaleWeek.Value + PowerLavel;
                }
                timerWeek.Stop();
                CheckParam2 = false;
                if (CheckParam1 = false && CheckParam2 == false)
                {
                    ButtonStartCoppy.IsEnabled = true;
                }

                using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(ExeptionMessage);
                }

                MessageBox.Show("Процесс копирования закончился!", "Уведомление!");
            }
            catch (Exception exp)
            {
                ExeptionMessage += "WeeklyCoppyCatalog " + exp.ToString() + "\n\n";
            }
        }
        #endregion

        #region Копирование месечное
        private async void MonthlyCoppyCatalog()
        {
            try
            {
                List<PathStruct> PathCollection = new List<PathStruct>();
                using (FileStream fs = new FileStream("data.xml", FileMode.Open))
                {
                    XmlSerializer xmlser = new XmlSerializer(typeof(SettingWindowViewModel));

                    try
                    {
                        WinSetting = (SettingWindowViewModel)xmlser.Deserialize(fs);
                        foreach (SettingCollectionModel item in WinSetting.SerializCollectionFolder.Where(x=>x.ParamWM == "Monthly"))
                        {
                            PathCollection.Add(new PathStruct() { CatalogName = item.LocationFolder, PathName = item.FolderName });
                        }
                    }

                    catch (Exception exp)
                    {

                    }
                }

                int CountPoint = PathCollection.Count();
                PowerTextMontle.Text = "0/" + CountPoint;
                double PowerLavel = ((double)100 / (double)CountPoint);
                int i = 0;

                foreach (PathStruct CopyPath in PathCollection)
                {
                    await Task.Run(() => CoppyCompressionCatalog(CopyPath.CatalogName, CopyPath.PathName));
                    i++;
                    PowerTextMontle.Text = i + "/" + CountPoint;
                    PowerScaleMontle.Value = PowerScaleMontle.Value + PowerLavel;
                }
                timerMonthly.Stop();

                CheckParam1 = false;
                if (CheckParam1 = false && CheckParam2 == false)
                {
                    ButtonStartCoppy.IsEnabled = true;
                }

            }
            catch (Exception exp)
            {
                ExeptionMessage += "MonthlyCoppyCatalog " + exp.ToString() + "\n\n";
            }
        }
#endregion

        #region Копирование каталога без сжатия
        private void CoppyCatalog(string CoppyCatalogPath, string NewPathName)
        {
            try
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName);
                foreach (string dirPath in Directory.GetDirectories(CoppyCatalogPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        Directory.CreateDirectory(dirPath.Replace(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + @"\"));
                    }
                    catch (Exception exp)
                    {
                        ExeptionMessage += "Ошибка копирования директории \n\n" + exp + "\n\n";
                    }

                }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Ошибка копирования директорий\n\n" + exp + "\n\n";
            }

            try
            {
                foreach (string newPath in Directory.GetFiles(CoppyCatalogPath, ".", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(newPath, newPath.Replace(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + @"\"), true);
                    }
                    catch (Exception exp)
                    {
                        ExeptionMessage += "Ошибка копирования файла\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + @"\" + " \n\n" +
                        newPath + "\n" + newPath.Replace(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName) + " \n\n" + exp + "\n\n";
                        }
                }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Ошибка копирования файлов директории\n\n" + exp + "\n\n";
            }

        }
        #endregion

        #region Копирование каталога с сжатием
        private async void CoppyCompressionCatalog(string CoppyCatalogPath, string NewPathName)
        {
            try
            {
                    FileInfo fi = new FileInfo(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    if (!fi.Exists)
                    {
                        ZipFile.CreateFromDirectory(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    }
                    else
                    {
                        File.Delete(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                        ZipFile.CreateFromDirectory(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    }
            }
            catch (IOException ioexp)
            {
                    try
                    {
                        CoppyCatalog(CoppyCatalogPath, NewPathName);
                        File.Delete(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                        ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + @"\", Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    }
                    catch (Exception exp)
                    {
                        ExeptionMessage += "Ошибка копирования 2-го файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + CurrentDate.ToShortDateString() + @"\" + NewPathName + "\n\n" + exp + "\n\n";
                    }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Ошибка копирования файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + "\n\n" + exp + "\n\n";
                }
        }
        #endregion

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.Show();
        }

        private void SetingButton_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow SettWind = new SettingWindow();

            using (FileStream fs = new FileStream("data.xml", FileMode.Open))
            {
                XmlSerializer xmlser = new XmlSerializer(typeof(SettingWindowViewModel));

                try
                {
                    WinSetting = (SettingWindowViewModel)xmlser.Deserialize(fs);
                    SettWind.DataContext = WinSetting;
                }

                catch (Exception exp)
                {
                    
                }
            }
            
            SettWind.Show();
        }
    }
}
