﻿using My_money.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.ViewModel
{
    [Serializable]
    public class ContainerAppData
    {
        public ObservableCollection<Record> Records { get; set; }
        public float Savings { get; set; }
        public float Balance { get; set; }

        public ObservableCollection<RecordByTypes> RecordsByTypes { get; set; }
        public ObservableCollection<string> Types { get; set; }
        public ObservableCollection<SavingsGoal> SavingsGoal { get; set;}
    }
}
