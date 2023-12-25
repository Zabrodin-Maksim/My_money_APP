using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using My_money.Model;

namespace My_money.ViewModel
{
    [Serializable]
    public class MainViewModel
    {
        public MainViewModel() {

            LoadRecords();

            Window mainWindow = Application.Current.MainWindow;

            if (mainWindow != null)
            {
                mainWindow.Closing += MainWindowClosing;
            }
        }

        private int banksum;
        public int Banksum { get { return banksum; } }
        
        private int totalCost;
        public int TotalCost { get { return totalCost; } }

        public ObservableCollection<Record> Records { get; set; }
        
    
        private void LoadRecords()
        {
            // Loading data from an XML file
            string fileName = "data.xml";

            if (File.Exists(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Record>));

                using (Stream stream = new FileStream(fileName, FileMode.Open))
                {
                    MainViewModel? loadedViewMpdel = serializer.Deserialize(stream) as MainViewModel;
                    
                    Records = loadedViewMpdel.Records;

                }
            }


            else
            {
                // If the file does not exist, create a new list of data
                ObservableCollection<Record> records = new ObservableCollection<Record>();

                records.Add(new Record { Cost = 100 });
                records.Add(new Record { Cost = 200 });
                records.Add(new Record { Cost = 2000 });
                records.Add(new Record { Cost = 2000 });

                Records = records;
            }

            CalculateCost();
        }


        private void CalculateCost()
        {
            for (int i = 0; Records.Count > i; i++)
            {
                totalCost += Records[i].Cost;
            }
        }



        private void SaveToXml(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Record>));
            
            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        private void MainWindowClosing(object? sender, CancelEventArgs e)
        {
            SaveToXml("data.xml");
        }

    }
}
