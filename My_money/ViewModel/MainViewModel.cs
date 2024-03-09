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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace My_money.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Records = new ObservableCollection<Record>();
            RecordsByTypes = new ObservableCollection<RecordByTypes>();
            SavingsGoals = new ObservableCollection<SavingsGoal>();

            LoadRecords();
            SortingRecordsDate();

            #region ViewModel
            addViewModel = new AddViewModel();
            historyViewModel = new HistoryViewModel(Records);
            planViewModel = new PlanViewModel(RecordsByTypes, Records);
            moneyBoxViewModel = new MoneyBoxViewModel(SavingsGoals);
            #endregion

            #region Commands
            NavCommand = new MyICommand<string>(OnNav);
            ExitCommand = new MyICommand<object>(OnExit);
            #endregion

            addViewModel.RecordAdded += OnRecordAdded;
            addViewModel.SavingsAdded += OnBankAdded;
            addViewModel.BalanceAdded += OnBalanceAdded;
            addViewModel.Back += OnNav;

            historyViewModel.BalanceBack += OnBalanceBack;

            #region Closing
            Window mainWindow = Application.Current.MainWindow;

            if (mainWindow != null)
            {
                mainWindow.Closing += MainWindowClosing;
            }
            #endregion

        }


        private void OnBalanceBack(int cost)
        {
            Balance += cost;
        }

        private void CalculateTotalSpending()
        {
            totalCost = 0;
            foreach (var record in listRecordsByDate)
            {
                TotalCost += record.Cost;
            }
            OnPropertyChanged(nameof(TotalCost));
        }

        private void SortingRecordsDate() //History View
        {
            List<Record> sortedList = Records.OrderByDescending(item => item.DateTimeOccurred).ToList();
            Records.Clear();
            foreach (var item in sortedList)
            {
                Records.Add(item);
            }
        }


        #region Properties
        //Moneybox
        private ObservableCollection<SavingsGoal> savingsGoals;
        public ObservableCollection<SavingsGoal> SavingsGoals {  get { return savingsGoals; } set { SetProperty(ref savingsGoals, value); } }

        //Dashboard
        private int totalCost;
        public int TotalCost { get { return totalCost; } set { SetProperty(ref totalCost, value); } }

        private int balance;
        public int Balance { get { return balance; } set { SetProperty(ref balance, value); } }

        private int savings;
        public int Savings { get { return savings; } set { SetProperty(ref savings, value); } }




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
                    Savings = Savings,
                    Balance = Balance,
                    RecordsByTypes = RecordsByTypes,
                    Types = TypesName.Values,
                    SavingsGoal = SavingsGoals
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
                        savings = appData.Savings;
                        balance = appData.Balance;
                        recordsByTypes = appData.RecordsByTypes;
                        TypesName.Values = appData.Types;
                        SavingsGoals = appData.SavingsGoal;
                    }
                }
            }
            else
            {
                // If the file does not exist, create a new list of data 
                savings = 0;
                balance = 0;

                recordsByTypes.Add(new RecordByTypes(TypesName.Values[0], 6000));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[1], 1700));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[2], 0));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[3], 5400));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[4], 750));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[5], 360));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[6], 450));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[7], 2000));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[8], 1000));
                recordsByTypes.Add(new RecordByTypes(TypesName.Values[9], 2000));

            }

            SortListByTypes();
            ViewModelBase.flagStartProg = true;
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
        private PlanView planView = new PlanView();
        private MoneyBoxView moneyBoxView = new MoneyBoxView();
        #endregion

        #region ViewModel
        private AddViewModel addViewModel;
        private HistoryViewModel historyViewModel;
        private PlanViewModel planViewModel;
        private MoneyBoxViewModel moneyBoxViewModel;
        #endregion

        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    SortListByTypes(); 
                    CurrentView = dashboardView;
                    break;

                case "AddRecord":
                    CurrentView = addView;
                    addView.DataContext = addViewModel;
                    break;

                case "History":
                    CurrentView = historyView;
                    historyView.DataContext = historyViewModel;
                    break;

                case "Plan":
                    CurrentView = planView;
                    planView.DataContext = planViewModel;
                    break;

                case "Moneybox":
                    CurrentView = moneyBoxView;
                    moneyBoxView.DataContext = moneyBoxViewModel;
                    break;
            }
        }
        #endregion


        #region OnAdd
        private void OnRecordAdded(Record newRecord)
        {
            Balance -= newRecord.Cost; 
            Records.Add(newRecord);

            OnNav("Dashboard");
        }

        private void OnBankAdded(int savings) 
        {
            Savings += savings;

            OnNav("Dashboard");
        }

        private void OnBalanceAdded(int balance)
        {
            Balance += balance;

            OnNav("Dashboard");
        }
        #endregion


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
                CalculateTotalSpending();
            }
            else
            {
                SelectedDate = DateTime.Now;
            }
        }
        #endregion

        #region Sorting List By Types
        private void SortListByTypes()
        {
            ChangePlanPeriod();
            CleanSpendByTypes();
            SortingRecordsByDate();
            if (listRecordsByDate != null)
            {

                foreach (var record in listRecordsByDate)
                {
                    for(int i = 0; i < TypesName.Values.Count; i++)
                    {
                        if (record.Type == TypesName.Values[i])
                        {
                            recordsByTypes.FirstOrDefault(item => item.Name == record.Type).Spend += record.Cost;
                            break;
                        }
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

        private void ChangePlanPeriod()
        {
            foreach (var types in recordsByTypes)
            {
                types.PlanByDatePeriod = SelectedSort;
            }
        }
        #endregion
    }
}
