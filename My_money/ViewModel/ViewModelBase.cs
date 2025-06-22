using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace My_money.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        static public bool flagStartProg = false;
        protected virtual void SetProperty<T>(ref T member, T val,
         [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return;

            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
