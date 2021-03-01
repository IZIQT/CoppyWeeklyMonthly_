using CoppyWeeklyMonthly.Common;
using NdbLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.IO.Compression;

namespace CoppyWeeklyMonthly
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly static OracleClientDatabase oraDb = new OracleClientDatabase(Connection.GetConnectionString());
        private readonly static SQLRequests sql = new SQLRequests();

        private string ExeptionMessage;

        #region Таймеры, классы и переменные
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
                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("Button_Click\n" +exp.ToString(), "Ошибка");
                //}
                //MessageBox.Show("Button_Click\n" + exp.ToString() + "\n\n");
            }

        }

        #region Копирование недельное
        private async void WeeklyCoppyCatalog()
        {//C:\COMMMON\dddd
            try
            {
                List<PathStruct> PathCollection = new List<PathStruct>()
            {
                new PathStruct(){ CatalogName = @"\\Duo\OraCopy\Backup\VT", PathName = "VT" }, //занят процессом
                new PathStruct(){ CatalogName = @"D:\Program Files", PathName = "D" },
                new PathStruct(){ CatalogName = @"J:\Homyak", PathName = "Homyak" },
                new PathStruct(){ CatalogName = @"J:\Sharkov", PathName = "Sharkov" },
                new PathStruct(){ CatalogName = @"J:\Веселков", PathName = "Веселков" },
                new PathStruct(){ CatalogName = @"J:\Зайцев", PathName = "Зайцев" },
                new PathStruct(){ CatalogName = @"J:\TRONIX_V", PathName = "TRONIX_V" },
                new PathStruct(){ CatalogName = @"J:\Vekshina", PathName = "Vekshina" },
                new PathStruct(){ CatalogName = @"J:\Yasashnev", PathName = "Yasashnev" },
                new PathStruct(){ CatalogName = @"J:\Yakimiv", PathName = "Yakimiv" },
                new PathStruct(){ CatalogName = @"J:\Воробьёва", PathName = "Воробьёва" },
                new PathStruct(){ CatalogName = @"J:\Guzov", PathName = "Guzov" },
                new PathStruct(){ CatalogName = @"J:\ZakharovDB", PathName = "ZakharovDB" },
                new PathStruct(){ CatalogName = @"J:\Tretyakov", PathName = "Tretyakov" },
                new PathStruct(){ CatalogName = @"J:\Текстовые документы", PathName = "Текстовые документы" },
                new PathStruct(){ CatalogName = @"J:\ОПЕЧАТЫВАНИЕ", PathName = "ОПЕЧАТЫВАНИЕ" },
                new PathStruct(){ CatalogName = @"J:\Электронщики", PathName = "Электронщики" },
                new PathStruct(){ CatalogName = @"J:\LocalNet", PathName = "LocalNet" },
                new PathStruct(){ CatalogName = @"O:\Ow", PathName = "OW" },
                new PathStruct(){ CatalogName = @"\\prima\installlog$\", PathName = "InstallData" }, //занят процессом
                new PathStruct(){ CatalogName = @"\\catia\Projects12.1.SP4", PathName = "Aveva(Projects12.1.SP4)" } //занят процессом
            };
                PowerTextWeek.Text = "0/" + PathCollection.Count();
                double PowerLavel = (double)100 / (double)PathCollection.Count();
                int CounterMonthly = 0;
                foreach (PathStruct CopyPath in PathCollection)
                {
                    //await Task.Run(() => CoppyCatalog(CopyPath.CatalogName, CopyPath.PathName));
                    await Task.Run(() => CoppyCompressionCatalog(CopyPath.CatalogName, CopyPath.PathName));
                    //CoppyCatalog(CopyPath.CatalogName, CopyPath.PathName);
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
                #region Исходный батник
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t - v4100m VT_DMP           \\Duo\OraCopy\Backup\VT
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t TRONIX_V J:\TRONIX_V
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t D        "D:\Program Files"
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Homyak J:\Homyak
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Sharkov J:\Sharkov
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Tr J:\Tr
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Vekshina J:\Vekshina
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Yasashnev J:\Yasashnev
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t "џЄЁ¬Ёў"     J:\Yakimiv
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Acad         "S:\Џа®Ја ¬¬л ЌЏЉЃ\ЌҐЄ®в®алҐ Їа®Ја ¬¬л ¤«п Acad"
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t "‚®а®Ўмсў " "J:\‚®а®Ўмсў "
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Guzov J:\Guzov
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t ZakharovDB J:\ZakharovDB
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Tretyakov J:\Tretyakov
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t "’ҐЄбв®ўлҐ ¤®Єг¬Ґ­вл"   "J:\’ҐЄбв®ўлҐ ¤®Єг¬Ґ­вл"
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t "ЋЏ…—Ђ’›‚ЂЌ€…"      "J:\ЋЏ…—Ђ’›‚ЂЌ€…"
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t LocalNet     "J:\LocalNet"
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t OW O:/ OW
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t Prima_new J:\Prima_new
                //"C:\Program Files\WinRAR\winRAR.exe" a - r - rr3p - t InstallData "\\prima\installlog$\InstallData.mdb"
                #endregion
            }
            catch (Exception exp)
            {
                ExeptionMessage += "WeeklyCoppyCatalog " + exp.ToString() + "\n\n";
                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("WeeklyCoppyCatalog " + exp.ToString(), "Ошибка");
                //}
                //MessageBox.Show("WeeklyCoppyCatalog " + exp.ToString(), "Ошибка");
            }
        }
        #endregion

        #region Копирование месечное
        private async void MonthlyCoppyCatalog()
        {
            try
            {
                List<PathStruct> PathCollection = new List<PathStruct>()
            {
                new PathStruct(){ CatalogName = @"\\Duo\OraCopy\Backup\Oradata\PT", PathName = "PT" },
            };

                int CountPoint = PathCollection.Count() + 3;
                PowerTextMontle.Text = "0/" + CountPoint;
                double PowerLavel = ((double)100 / (double)CountPoint);
                int i = 0;

                DataSet vhod = oraDb.LoadDataSet(sql.SQL_GetVhodID(DateTime.Now.ToString("MM.yyyy")), "PPUNPKB_ACCESS");
                DataSet isvhod = oraDb.LoadDataSet(sql.SQL_GetIshodID(DateTime.Now.ToString("MM.yyyy")), "PPUNPKB_ACCESS");

                await Task.Run(() =>
                {
                    MailCoppy(vhod, "vhod");
                });
                i++;
                PowerTextMontle.Text = i + "/" + CountPoint;
                PowerScaleMontle.Value = PowerScaleMontle.Value + PowerLavel;
                await Task.Run(() =>
                {
                    MailCoppy(isvhod, "ishod");
                });
                i++;
                PowerTextMontle.Text = i + "/" + CountPoint;
                PowerScaleMontle.Value = PowerScaleMontle.Value + PowerLavel;
                await Task.Run(() =>
                {
                    FileInfo fi = new FileInfo(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта.zip");
                    if (!fi.Exists)
                    {
                        ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта", Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта.zip");
                    }
                    else
                    {
                        File.Delete(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта.zip");
                        ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта", Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта.zip");
                    }
                });
                i++;
                PowerTextMontle.Text = i + "/" + CountPoint;
                PowerScaleMontle.Value = PowerScaleMontle.Value + PowerLavel;

                foreach (PathStruct CopyPath in PathCollection)
                {
                    //await Task.Run(() => CoppyCatalog(CopyPath.CatalogName, CopyPath.PathName));
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

                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine(ExeptionMessage);
                //}

                //MessageBox.Show("Процесс копирования закончился!", "Уведомление!");

            }
            catch (Exception exp)
            {
                //MessageBox.Show("MonthlyCoppyCatalog " + exp.ToString(), "Ошибка");

                ExeptionMessage += "MonthlyCoppyCatalog " + exp.ToString() + "\n\n";

                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("MonthlyCoppyCatalog " + exp.ToString());
                //}

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
                        //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                        //{
                        //    sw.WriteLine("Ошибка копирования директории \n\n" + exp);
                        //}
                        //MessageBox.Show("Ошибка копирования директории \n\n" + exp, "Ошибка!");
                    }

                }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Ошибка копирования директорий\n\n" + exp + "\n\n";
                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("Ошибка копирования директорий\n\n" + exp);
                //}
                //MessageBox.Show("Ошибка копирования директорий\n\n" + exp, "Ошибка!");
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
                        //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                        //{
                        //    sw.WriteLine("Ошибка копирования файла\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + @"\" + " \n\n" +
                        //newPath +"\n" + newPath.Replace(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName) + " \n\n" + exp);
                        //}
                        //MessageBox.Show("Ошибка копирования файла\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + @"\" + " \n\n" +
                        //newPath + "\n" + newPath.Replace(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName) + " \n\n" + exp, "Ошибка!");
                    }
                }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Ошибка копирования файлов директории\n\n" + exp + "\n\n";
                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("Ошибка копирования файлов директории\n\n" + exp);
                //}
                //MessageBox.Show("Ошибка копирования файлов директории\n\n" + exp, "Ошибка!");
            }

        }
        #endregion

        #region Копирование каталога с сжатием
        private async void CoppyCompressionCatalog(string CoppyCatalogPath, string NewPathName)
        {
            try
            {
                //Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName);
                FileInfo fi = new FileInfo(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                if (!fi.Exists)
                {
                    ZipFile.CreateFromDirectory(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                }
                else
                {
                    File.Delete(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    ZipFile.CreateFromDirectory(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    //MessageBox.Show(Directory.GetCurrentDirectory() + @"\" + NewPathName + ".zip" +" -Указанный файл существует.\nПапка должна быть пуста перед копированием.");
                }
                
            }
            catch (IOException ioexp)
            {
                try
                {
                    //await Task.Run(() => CoppyCatalog(CoppyCatalogPath, NewPathName));
                    CoppyCatalog(CoppyCatalogPath, NewPathName);
                    File.Delete(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                    ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + @"\", Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\" + NewPathName + ".zip");
                }
                catch(Exception exp)
                {
                    ExeptionMessage += "Ошибка копирования 2-го файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + CurrentDate.ToShortDateString() + @"\" + NewPathName + "\n\n" + exp + "\n\n";
                    //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                    //{
                    //    sw.WriteLine("Ошибка копирования 2-го файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + CurrentDate.ToShortDateString() + @"\" + NewPathName + "\n\n" + exp);
                    //}
                    //MessageBox.Show("Ошибка копирования 2-го файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + CurrentDate.ToShortDateString() + @"\" + NewPathName + "\n\n" + exp, "Ошибка!");
                }
            }
            catch (Exception exp)
            {
                ExeptionMessage += "Ошибка копирования файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + "\n\n" + exp + "\n\n";
                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("Ошибка копирования файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + "\n\n" + exp);
                //}
                //MessageBox.Show("Ошибка копирования файлов директории\n\n" + CoppyCatalogPath + "\n" + Directory.GetCurrentDirectory() + @"\" + NewPathName + "\n\n" + exp, "Ошибка!");
            }
        }
        #endregion

        #region Старое копирование почты (непереписанное)
        private void MailCoppyWeeklyOld()
        {
            //Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + NewPathName);
            //File.Copy(newPath, newPath.Replace(CoppyCatalogPath, Directory.GetCurrentDirectory() + @"\" + NewPathName), true);
            List<string> CatalogCollection = new List<string>()
            {
                @"Входящие",
                @"Исходящие",
                @"Конфиденциальные",
                @"Конфиденциальные\Бухгалтерия",
                @"Конфиденциальные\ОГК",
                @"Конфиденциальные\Отдел Кадров",
                @"Конфиденциальные\Руководство",
                @"Конфиденциальные\ФЭО",
                @"Конфиденциальные\Бухгалтерия\Входящие",
                @"Конфиденциальные\Бухгалтерия\Исходящие",
                @"Конфиденциальные\Бухгалтерия\Приказы",
                @"Конфиденциальные\ОГК\Входящие",
                @"Конфиденциальные\ОГК\Исходящие",
                @"Конфиденциальные\Отдел Кадров\Входящие",
                @"Конфиденциальные\Отдел Кадров\Исходящие",
                @"Конфиденциальные\Отдел Кадров\Приказы",
                @"Конфиденциальные\Руководство\Входящие",
                @"Конфиденциальные\Руководство\Исходящие",
                @"Конфиденциальные\Руководство\Приказы",
                @"Конфиденциальные\ФЭО\Входящие",
                @"Конфиденциальные\ФЭО\Исходящие",
                @"Конфиденциальные\ФЭО\Приказы"
            };
            int b1max_vh = 0;
            int b1min_vh = 0;
            int ListCount = 0;
            string b1path_vh = "";
            string file_in = "";
            string file_out = "";
            string Path2 = "";
            string Path1 = "";
            if (b1max_vh > 0)
            {


                for (int a1 = 0; b1min_vh == b1max_vh; a1++)
                {
                    int alpha = (a1 - 50000) / 10000;
                    int beta = (a1 - 50000) % 10000;
                    int ceta = beta % 100;
                    beta = beta / 100;
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + @"\Конфиденциальные\Бухгалтерия\Входящие\" + (alpha * 10000));
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + @"\Конфиденциальные\Бухгалтерия\Входящие\" + (alpha * 10000) + @"\" + (beta * 100));
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + @"\Конфиденциальные\Бухгалтерия\Входящие\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + ceta);
                    string rezer_x = b1path_vh + @"\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta);


                    //Form1.Dir1.Path = b1path_vh + @"\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta);
                    Path1 = b1path_vh + @"\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta);

                    rezer_x = b1path_vh + @"\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta);
                    //Form1.F1.Path = b1path_vh + @"\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta);
                    Path2 = b1path_vh + @"\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta);
                    ListCount = Directory.GetDirectories(Path2).Count();
                    if (ListCount > 0)
                    {
                        foreach (string dirPath in Directory.GetDirectories(Path2, "*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                file_in = ListCount + @"\" + dirPath;
                                file_out = Directory.GetCurrentDirectory() + @"\" + @"\Конфиденциальные\Бухгалтерия\Исходящие\" + (alpha * 10000) + @"\" + (beta * 100) + @"\" + (ceta) + @"\";
                                if (file_in.Remove(0, rezer_x.Count()) == rezer_x)
                                {
                                    File.Copy(file_in, file_out, true);
                                }
                            }
                            catch (Exception exp)
                            {
                                ExeptionMessage += "Ошибка копирования директории \n\n" + exp + "\n\n";
                                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                                //{
                                //    sw.WriteLine("Ошибка копирования директории \n\n" + exp);
                                //}
                                //MessageBox.Show("Ошибка копирования директории \n\n" + exp, "Ошибка!");
                            }

                        }
                    }
                }
            }
        }
        #endregion

        #region Копирование почты
        private void MailCoppy(DataSet DataMail, string param)
        {
            try
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта");
                foreach (DataRow row in DataMail.Tables[0].Rows)
                {
                    string dirName = Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Почта\" + row["IDV"].ToString() + '_' + row["INP_N"].ToString();
                    Directory.CreateDirectory(dirName);
                    try
                    {
                        File.Copy(row["FILES"].ToString(), dirName + @"\" + row["name_file"].ToString(), true);
                    }
                    catch (FileNotFoundException exp)
                    {
                        //ExeptionMessage += row["INP_N"].ToString() + "\t" + row["name_file"].ToString() + "\n\n";
                        //MessageBox.Show(row["INP_N"].ToString() + "\t" + row["name_file"].ToString() + "\n\n", "Ошибка");
                        using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\LogFileNotFoundException" + param + ".txt", true, System.Text.Encoding.Default))
                        {
                            //sw.WriteLine(row["IDV"].ToString() + '_' + row["INP_N"].ToString() + row["FILES"].ToString() + "   ");
                            sw.WriteLine(row["INP_N"].ToString() + "\t" + row["name_file"].ToString() + "   ");
                        }
                    }
                    catch (Exception exp)
                    {
                        ExeptionMessage += "MailCoppy copy \n\n" + row["FILES"].ToString() + @"\n\n" + dirName + @"\" + row["name_file"].ToString() + "\n\n" + exp.ToString() + "\n\n";
                        //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                        //{
                        //    sw.WriteLine("MailCoppy copy \n\n" + row["FILES"].ToString() + @"\n\n" + dirName + @"\" + row["name_file"].ToString() + "\n\n" + exp.ToString());
                        //}
                        //MessageBox.Show("MailCoppy copy \n\n" + row["FILES"].ToString() + @"\n\n" + dirName + @"\" + row["name_file"].ToString() + "\n\n" + exp.ToString(), "Ошибка");
                    }
                }                    
            }
            catch (Exception exp)
            {
                ExeptionMessage += "MailCoppy " + exp.ToString() + "\n\n";
                //using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\" + CurrentDate.ToShortDateString() + @"\Exception.txt", true, System.Text.Encoding.Default))
                //{
                //    sw.WriteLine("MailCoppy " + exp.ToString());
                //}
                //MessageBox.Show("MailCoppy " + exp.ToString(), "Ошибка");
            }
        }
        #endregion

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.Show();
        }
    }
}
