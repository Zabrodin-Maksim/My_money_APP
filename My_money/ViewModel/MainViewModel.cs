using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
            RecordsByTypes = new ObservableCollection<RecordByTypes>();

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
            addViewModel.BankAdded += OnBankAdded;
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
        public int Banksum { get { return banksum; } set { SetProperty(ref banksum, value); } }


        private int totalCost;
        public int TotalCost { get { return totalCost; } }


        private ObservableCollection<Record> records;
        public ObservableCollection<Record> Records { get { return records; } 
            set 
            { 
                SetProperty(ref records, value);
            } 
        }


        #region Sort List

        private ObservableCollection<RecordByTypes> recordsByTypes;
        public ObservableCollection<RecordByTypes> RecordsByTypes { get { return recordsByTypes; } set { recordsByTypes = value; } }

        private List<Record> listRecordsByDate;
        public List<Record> ListRecordsByDate { get { return listRecordsByDate; } set { listRecordsByDate = value; } }

        private int selectedSort = 1;
        public int SelectedSort 
        { 
            get { return selectedSort; } 
            set 
            { 
                selectedSort = value;
                SortListByTypes();
            } 
        }

        private DateTime? selectedDate = DateTime.Now;
        public DateTime? SelectedDate
        {
            get { return selectedDate; }
            set 
            { 
                selectedDate = value;
                SortListByTypes();
            }
        }
        #endregion
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
                    RecordsByTypes = RecordsByTypes
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
                        recordsByTypes = appData.RecordsByTypes;
                    }
                }
            }
            else
            {
                // If the file does not exist, create a new list of data for demo
                banksum = 5000;

                Records.Add(new Record(100, DateTime.Now, TypesRecord.Groceries));
                Records.Add(new Record(200, DateTime.Now, TypesRecord.Groceries));
                Records.Add(new Record(2000, DateTime.Now, TypesRecord.Entertainment));
                Records.Add(new Record(2000, DateTime.Now, TypesRecord.Other));

                recordsByTypes.Add(new RecordByTypes(TypesRecord.Groceries, 6000));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Cafe, 1700));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Study, 0));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Housing, 5400));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Phone, 750));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Washing, 360));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Haircut, 450));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Car, 2000));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Entertainment, 1000));
                recordsByTypes.Add(new RecordByTypes(TypesRecord.Other, 2000));

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
                    SortListByTypes(); 
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

        private void OnBankAdded(int bank) 
        {
            Banksum += bank;

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


        #region Sorting Records By Date
        private void SortingRecordsByDate()
        {
            if (SelectedDate.HasValue)
            {
                listRecordsByDate = Records.ToList();
                switch (selectedSort)
                {

                    // Day
                    case 0:
                        for (int i = listRecordsByDate.Count - 1; i >= 0; i--)
                        {
                            var item = listRecordsByDate[i];
                            if (item.DateTimeOccurred.Value.Day != SelectedDate.Value.Day || item.DateTimeOccurred.Value.Month != SelectedDate.Value.Month || item.DateTimeOccurred.Value.Year != SelectedDate.Value.Year)
                            {
                                listRecordsByDate.RemoveAt(i);
                            }
                        }
                        break;
                    // Month
                    case 1:
                        for (int i = listRecordsByDate.Count - 1; i >= 0; i--)
                        {
                            var item = listRecordsByDate[i];
                            if (item.DateTimeOccurred.Value.Month != SelectedDate.Value.Month || item.DateTimeOccurred.Value.Year != SelectedDate.Value.Year)
                            {
                                listRecordsByDate.RemoveAt(i);
                            }
                        }
                        break;
                    // Year
                    case 2:
                        for (int i = listRecordsByDate.Count - 1; i >= 0; i--)
                        {
                            var item = listRecordsByDate[i];
                            if (item.DateTimeOccurred.Value.Year != SelectedDate.Value.Year)
                            {
                                listRecordsByDate.RemoveAt(i);
                            }
                        }
                        break;
                }
            }
            else
            {
                SelectedDate = DateTime.Now;
            }
        }
        #endregion

        #region Sorting Records By Types
        private void SortListByTypes()
        {
            CleanSpendByTypes();
            SortingRecordsByDate();
            if (listRecordsByDate != null)
            {

                foreach (var record in listRecordsByDate)
                {
                    switch(record.Type)
                    {
                        case TypesRecord.Groceries:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Cafe:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Study:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Housing:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Phone:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Washing:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Haircut:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Car:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Entertainment:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;

                        case TypesRecord.Other:
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;
                        
                    }
                }
                
            }
            else
            {
                MessageBox.Show("You don't have any records!", "Error Detected in records list", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void CleanSpendByTypes()
        {
            foreach (var types in recordsByTypes)
            {
                types.Spend = 0;
            }
        }
        #endregion
    }
}
