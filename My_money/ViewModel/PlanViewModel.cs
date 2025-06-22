using My_money.Model;
using System.Collections.ObjectModel;
using System.Windows;

namespace My_money.ViewModel
{
    public class PlanViewModel : ViewModelBase
    {
        private ObservableCollection<RecordByTypes> recordByTypes;
        public ObservableCollection<RecordByTypes> RecordByTypes {  get { return recordByTypes; } set { SetProperty(ref recordByTypes, value); } }

        public RecordByTypes selectedItem { get; set; }

        public MyICommand<object> AddCommand { get; set; }
        public MyICommand<object> DeleteCommand { get; set; }

        private ObservableCollection<Record> records;

        public PlanViewModel(ObservableCollection<RecordByTypes> recordByTypes, ObservableCollection<Record> records) 
        {
            RecordByTypes = recordByTypes;
            this.records = records;

            AddCommand = new MyICommand<object>(OnAdd);
            DeleteCommand = new MyICommand<object>(OnDelete);
        }

        private void OnDelete(object par)
        {
            if (selectedItem != null)
            {
                if(selectedItem.Name == "Other")
                {
                    MessageBox.Show("You cannot delete the 'Other' record type as it is a universal type!", "Warning: Error Detected in Delete Other type", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    for (int i = 0; i < TypesName.Values.Count; i++)
                    {
                        if (TypesName.Values[i] == selectedItem.Name)
                        {
                            TypesName.Values.RemoveAt(i);
                            break;
                        }
                    }
                    Change(selectedItem.Name);
                    MessageBox.Show("All records of type " + selectedItem.Name + " have been moved under the 'Other' type.", "Information: Successful deletion of the type" + selectedItem.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                    RecordByTypes.Remove(selectedItem);
                }
            }
            else
            {
                MessageBox.Show("Please, select the Item", "Error Detected in Selected Item", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void OnAdd(object par)
        {
            RecordByTypes.Add(new RecordByTypes("New Type", 0));
        }
        private void Change(string nameType)
        {
            foreach (var record in records)
            {
                if(record.Type == nameType)
                {
                    record.Type = "Other";
                }
            }
        }
    }
}
