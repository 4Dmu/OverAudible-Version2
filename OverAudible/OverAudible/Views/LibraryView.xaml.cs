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
using OverAudible.API;

namespace OverAudible.Views
{
    [Inject(InjectionType.Transient)]
    public partial class LibraryView : ShellPage
    {
        public LibraryViewModel viewModel => this.DataContext as LibraryViewModel;

        public LibraryView(LibraryViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            this.Loaded += async (s, e) =>
            {
                await viewModel.LoadAsync();
            };
        }

        private async void CollectionBookInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Collection col)
                {
                    await Shell.Current.GoToAsync(nameof(CollectionDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"CollectionParam", col }
                    });
                }
            }
        }

        private async void WishlistBookInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Item item)
                {
                    /*
                    var client = await ApiClient.GetInstance();
                    Item item2;
                    try
                    {
                        item2 = await client.GetLibraryItemAsync(item.Asin, AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
                    }
                    catch
                    {
                        item2 = await client.GetCatalogItemAsync(item.Asin, AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
                    }
                    */
                    await Shell.Current.GoToAsync(nameof(BookDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"ItemParam", item }
                    });
                }
            }
        }

        private async void LibraryBookInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border c)
            {
                if (c.DataContext is Item item)
                {
                    await Shell.Current.GoToAsync(nameof(BookDetailsView), true, ShellWindow.Direction.Left, new Dictionary<string, object>
                    {
                        {"ItemParam", item }
                    });
                }
            }
        }

        private void CollectionOptionsInstance_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Collection col)
                {
                    viewModel.CollectionOptionsCommand.Execute((col.Title, col.CollectionId));
                }
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.AddToCartCommand.Execute(i);
                }
            }
        }

        private void AddToWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.AddToWishlistCommand.Execute(i);
                }
            }
        }

        private void RemoveFromWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.RemoveFromWishlistCommand.Execute(i);
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.PlayCommand.Execute(i);
                }
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    viewModel.StandardCommands.DownloadCommand.Execute(i);
                }
            }
            
        }

        private void Sample_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.DataContext is Item i)
                {
                    if (viewModel.IsPlayingSample)
                    {
                        b.Content = "Sample";
                    }
                    else
                    {
                        b.Content = "Stop";
                    }

                    viewModel.SampleCommand.Execute(i);
                }
            }
        }
    }


}
