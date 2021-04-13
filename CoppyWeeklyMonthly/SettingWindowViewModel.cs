using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace CoppyWeeklyMonthly
{
    public class SettingCollectionModel:ViewModelBase
    {
        string locationFolder, folderName, paramWM;
        List<string> paramWMCollection;

        public string LocationFolder
        {
            get
            {
                return locationFolder;
            }
            set
            {
                locationFolder = value;
                OnPropertyChanged(nameof(LocationFolder));
            }
        }

        public string FolderName
        {
            get
            {
                return folderName;
            }
            set
            {
                folderName = value;
                OnPropertyChanged(nameof(FolderName));
            }
        }

        public string ParamWM
        {
            get
            {
                return paramWM;
            }
            set
            {
                paramWM = value;
                OnPropertyChanged(nameof(ParamWM));
            }
        }

        [XmlIgnore]
        public List<string> ParamWMCollection
        {
            get
            {
                return paramWMCollection = new List<string>() { "Weekly", "Monthly" };
            }
            set
            {
                paramWMCollection = new List<string>() { "Weekly", "Monthly" };
            }
        }
}

    
    [Serializable]
    public partial class SettingWindowViewModel : ViewModelBase
    {
        private ObservableCollection<SettingCollectionModel> serializCollectionFolder;

        [XmlArray, XmlArrayItem(typeof(SettingCollectionModel), ElementName = "Podp")]
        public ObservableCollection<SettingCollectionModel> SerializCollectionFolder
        {
            get
            {
                return serializCollectionFolder;
            }
            set
            {
                serializCollectionFolder = value;
                OnPropertyChanged(nameof(SerializCollectionFolder));
            }
        }
        [XmlIgnore]
        public static ObservableCollection<string> ParamWMCollection2;
        [XmlIgnore]
        public ICommand Button_Click { get; set; }

        public SettingWindowViewModel()
        {
            ParamWMCollection2 = new ObservableCollection<string>() { "Weekly", "Monthly" };
            SerializCollectionFolder = new ObservableCollection<SettingCollectionModel>();
            Button_Click = new RelayCommand(Button_ClickExecute);

        }

        private void Button_ClickExecute(object obj)
        {
            XmlSerializer x = new XmlSerializer(typeof(SettingWindowViewModel));
            using (FileStream fs = new FileStream("data.xml", FileMode.Create))
            {
                x.Serialize(fs, this);
            }
        }
    }
}
