using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Model
{
    public class TypesName
    {
        public static List<string> Values { get; set; } = new List<string>
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
