using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using My_money.Model;
using My_money.Views;

namespace My_money.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        # region Fields and Properties
        private int banksum;
        public int Banksum { get { return banksum; } }

        private int totalCost;
        public int TotalCost { get { return totalCost; } }

        public ObservableCollection<Record> Records { get; set; }
        #endregion

        #region Command
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand<object> AddCommand { get; private set; }
        #endregion

        public MainViewModel()
        {
            Records = new ObservableCollection<Record>();

            LoadRecords();

            NavCommand = new MyICommand<string>(OnNav);
            AddCommand = new MyICommand<object>(OnAdd);

            Window mainWindow = Application.Current.MainWindow;

            if (mainWindow != null)
            {
                mainWindow.Closing += MainWindowClosing;
            }
        }

        private void CalculateTotalSpending()
        {
            totalCost = 0;

            foreach (var record in Records)
            {
                totalCost += record.Cost;
            }

            OnPropertyChanged(nameof(TotalCost));
        }

        #region Start and Save
        private void SaveToXml(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ContainerAppData));

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                ContainerAppData appData = new ContainerAppData
                {
                    Records = Records,
                    Banksum = Banksum,
                };

                serializer.Serialize(stream, appData);
            }
        }

        private void LoadRecords()
        {
            // Loading data from an XML file
            string fileName = "data.xml";

            if (File.Exists(fileName))
            {
                using (Stream stream = new FileStream(fileName, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ContainerAppData));
                    ContainerAppData? appData = serializer.Deserialize(stream) as ContainerAppData;

                    if (appData != null)
                    {
                        Records = appData.Records;
                        banksum = appData.Banksum;
                    }
                }
            }
            else
            {
                // If the file does not exist, create a new list of data for demo
                banksum = 5000;

                Records.Add(new Record { Cost = 100 });
                Records.Add(new Record { Cost = 200 });
                Records.Add(new Record { Cost = 2000 });
                Records.Add(new Record { Cost = 2000 });

            }

            CalculateTotalSpending();
        }
        #endregion

        private void MainWindowClosing(object? sender, CancelEventArgs e)
        {
            SaveToXml("data.xml");
        }

        #region NAVIGATION

        private DashboardView dashboardView = new DashboardView();

        private UserControl currentView;
        public UserControl CurrentView
        {
            get { return currentView; }
            set { SetProperty(ref currentView, value); }
        }

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    CurrentView = dashboardView;
                    break;
                //case "History":
                //default:
                //    CurrentView = dashboardView;
                //    break;
            }
        }
        #endregion

        #region ADDbt
        
        private void OnAdd(object parametr)
        {
            Records.Add(new Record { Cost = 1 });
            CalculateTotalSpending();
        }
        #endregion
    }
}
