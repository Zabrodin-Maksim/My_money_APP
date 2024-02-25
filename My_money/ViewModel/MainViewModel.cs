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
using System.Windows.Input;
using System.Xml.Serialization;
using My_money.Model;
using My_money.Views;

namespace My_money.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Records = new ObservableCollection<Record>();
            LoadRecords();

            #region ViewModel
            addViewModel = new AddViewModel();
            historyViewModel = new HistoryViewModel(Records);
            #endregion

            #region Commands
            NavCommand = new MyICommand<string>(OnNav);
            ExitCommand = new MyICommand<object>(OnExit);
            #endregion

            addViewModel.RecordAdded += OnRecordAdded;
            addViewModel.BackM += OnNav;

            #region Closing
            Window mainWindow = Application.Current.MainWindow;

            if (mainWindow != null)
            {
                mainWindow.Closing += MainWindowClosing;
            }
            #endregion
        }

        private void CalculateTotalSpending()
        {
            totalCost = 0;

            foreach (var record in Records)
            {
                totalCost += record.Cost;
            }

            //OnPropertyChanged(nameof(TotalCost));
        }

        # region Fields and Properties
        private int banksum;
        public int Banksum { get { return banksum; } }

        private int totalCost;
        public int TotalCost { get { return totalCost; } }

        private ObservableCollection<Record> records;
        public ObservableCollection<Record> Records { get { return records; } 
            set 
            { 
                records = value;
            } 
        }

        #endregion

        #region Commands
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand<object> ExitCommand { get; private set; }
        #endregion


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

        private void MainWindowClosing(object? sender, CancelEventArgs e)
        {
            SaveToXml("data.xml");
        }
        #endregion


        #region NAVIGATION

        private UserControl currentView;
        public UserControl CurrentView
        {
            get { return currentView; }
            set 
            {
                SetProperty(ref currentView, value);
            }
        }
        
        #region Views
        private DashboardView dashboardView = new DashboardView();
        private AddView addView = new AddView();
        private HistoryView historyView = new HistoryView();
        #endregion

        #region ViewModel
        private AddViewModel addViewModel;
        private HistoryViewModel historyViewModel;
        #endregion

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    CalculateTotalSpending();
                    CurrentView = dashboardView;
                    break;

                case "AddRecord":
                    CurrentView = addView;
                    addView.DataContext = addViewModel;
                    break;

                case "History":
                default:
                    CurrentView = historyView;
                    historyView.DataContext = historyViewModel;
                    break;
            }
        }
        #endregion

        #region OnAdd
        private void OnRecordAdded(Record newRecord)
        {
            
            Records.Add(newRecord);
            CalculateTotalSpending();

            OnNav("Dashboard");
        }
        #endregion

        //private void SortingRecordsDate()
        //{
        //    List<Record> sortedList = Records.OrderByDescending(item => item.DateTimeOccurred).ToList();
        //    Records.Clear();
        //    foreach (var item in sortedList)
        //    {
        //        Records.Add(item);
        //    }
        //}

        #region Exit
        private void OnExit(object param)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
