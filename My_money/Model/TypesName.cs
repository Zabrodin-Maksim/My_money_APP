using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
