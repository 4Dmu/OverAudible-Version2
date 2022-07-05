using OverAudible.ViewModels;
using ShellUI.Attributes;
using ShellUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OverAudible.Models;

namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class CollectionDetailsView : ShellPage
    {
        public CollectionDetailsViewModel viewModel => this.DataContext as CollectionDetailsViewModel;

        public CollectionDetailsView(CollectionDetailsViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }

        private async void BookInstance_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Item item)
                {
                    await Shell.Current.GoToAsync(nameof(BookDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"ItemParam", item }
                    });

                    Shell.Current.OvverideBackButton = true;

                    Shell.Current.BackButtonCommand = new ShellUI.Commands.AsyncRelayCommand(async () =>
                    {
                        await Shell.Current.GoToAsync(nameof(CollectionDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                        {
                            {"CollectionParam", viewModel.Collection }
                        });

                        Shell.Current.OvverideBackButton = true;

                        Shell.Current.BackButtonCommand = new ShellUI.Commands.AsyncRelayCommand(async () =>
                        {
                            await Shell.Current.GoToAsync(nameof(LibraryView));
                        });

                    });
                }
            }
        }
    }
}
