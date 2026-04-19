using My_money.ViewModel;
using System.Windows.Controls;

namespace My_money.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void CurrentPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm && sender is PasswordBox box)
            {
                vm.CurrentPassword = box.Password;
            }
        }

        private void NewPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm && sender is PasswordBox box)
            {
                vm.NewPassword = box.Password;
            }
        }

        private void ConfirmPasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm && sender is PasswordBox box)
            {
                vm.ConfirmPassword = box.Password;
            }
        }
    }
}
