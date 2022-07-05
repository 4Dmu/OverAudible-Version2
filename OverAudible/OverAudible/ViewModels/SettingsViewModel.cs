using CommunityToolkit.Mvvm.Input;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public partial class SettingsViewModel : ViewModelBase
    {

        [RelayCommand]
        void ToogleTheme()
        {
            Shell.Current.ToggleTheme();
        }

        [RelayCommand]
        void ManageAccount()
        {
            var destinationurl = "https://www.audible.com/account";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }

        [RelayCommand]
        void LogOut()
        {
            File.Delete(OverAudible.API.UserSetup.IDENTITY_FILE_PATH);
            App.Current.Restart();
        }
    }
}
