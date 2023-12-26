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

        private int banksum;
        public int Banksum { get { return banksum; } }
        
        private int totalCost;
        public int TotalCost { get { return totalCost; } }

        public ObservableCollection<Record> Records { get; set; }


        public MainViewModel()
        {

            LoadRecords();

            Window mainWindow = Application.Current.MainWindow;

            if (mainWindow != null)
            {
                mainWindow.Closing += MainWindowClosing;
            }
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

                    if(appData != null)
                    {
                        Records = appData.Records;
                        banksum = appData.Banksum;
                    }

                }
            }
            else
            {
                // If the file does not exist, create a new list of data for demo
                ObservableCollection<Record> records = new ObservableCollection<Record>();

                banksum = 5000;

                records.Add(new Record { Cost = 100 });
                records.Add(new Record { Cost = 200 });
                records.Add(new Record { Cost = 2000 });
                records.Add(new Record { Cost = 2000 });

                Records = records;

            }

            CalculateCost();
        }



        private void MainWindowClosing(object? sender, CancelEventArgs e)
        {
            SaveToXml("data.xml");
        }

    }
}
