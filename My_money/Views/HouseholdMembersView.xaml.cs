using My_money.ViewModel;
using System.Windows.Controls;

namespace My_money.Views
{
    public partial class HouseholdMembersView : UserControl
    {
        public HouseholdMembersView()
        {
            InitializeComponent();
        }

        private void ChildInitialPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is HouseholdMembersViewModel vm && sender is PasswordBox box)
            {
                vm.ChildInitialPassword = box.Password;
            }
        }

        private void NewChildPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is HouseholdMembersViewModel vm && sender is PasswordBox box)
            {
                vm.NewChildPassword = box.Password;
            }
        }
    }
}
