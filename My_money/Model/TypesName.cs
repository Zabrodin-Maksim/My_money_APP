using System.Collections.ObjectModel;

namespace My_money.Model
{
    public class TypesName
    {
        public static ObservableCollection<string> Values { get; set; } = new ObservableCollection<string>
        {
            "Groceries",
            "Cafe",
            "Study",
            "Housing",
            "Phone",
            "Washing",
            "Haircut",
            "Car",
            "Entertainment",
            "Other"
        };
    }
}
