using My_money.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Model
{
    [Serializable]
    public class RecordByTypes : ViewModelBase
    {
        private TypesRecord name;
        public TypesRecord Name { get { return name; } set { name = value; } }

        private int spend;
        public int Spend { get { return spend; } set { SetProperty(ref spend, value); } }

        private int plan;
        public int Plan { get { return plan; } set { plan = value; SetProperty(ref plan, value); } }

        public RecordByTypes() { }

        public RecordByTypes(TypesRecord name, int plan)
        {
            this.name = name;
            this.plan = plan;
        }
    }
}
