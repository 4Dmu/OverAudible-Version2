using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OverAudible.API;
using OverAudible.Services;
using OverAudible.Views;
using ShellUI;
using ShellUI.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OverAudible
{
    public partial class App : Application
    {
        IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAutoMapper(typeof(App).Assembly);
                    services.AddSingleton<MediaPlayer>();
                    services.AutoRegisterDependencies(this.GetType().Assembly.GetTypes());
                })
                .Build();
        }

        protected async override void OnStartup(StartupEventArgs e)
        {
            _host.Start();

            Shell.SetServiceProvider(_host.Services);
            
            Routing.RegisterRoute(nameof(HomeView), typeof(HomeView));
            Routing.RegisterRoute(nameof(LibraryView), typeof(LibraryView));
            Routing.RegisterRoute(nameof(BrowseView), typeof(BrowseView));
            Routing.RegisterRoute(nameof(CartView), typeof(CartView));
            Routing.RegisterRoute(nameof(SettingsView), typeof(SettingsView));
            Routing.RegisterRoute(nameof(BookDetailsView), typeof(BookDetailsView));
            Routing.RegisterRoute(nameof(CollectionDetailsView), typeof(CollectionDetailsView));
            Routing.RegisterRoute(nameof(NewCollectionModal), typeof(NewCollectionModal));
            Routing.RegisterRoute(nameof(FilterModal), typeof(FilterModal));

            MainWindow = new Shell()
            {
                Title = "OverAudible",
                UseSecondTitleBar = true,
                FlyoutIconVisibility = FlyoutIconVisibility.BottomBar,
                FLyoutNavigationDuration = new Duration(TimeSpan.FromSeconds(.05))
            }
            .AddFlyoutItem(new FlyoutItem("Home",nameof(HomeView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Home))
            .AddFlyoutItem(new FlyoutItem("Library",nameof(LibraryView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Books))
            .AddFlyoutItem(new FlyoutItem("Browse",nameof(BrowseView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Search))
            .AddFlyoutItem(new FlyoutItem("Cart", nameof(CartView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Cart))
            .AddFlyoutItem(new FlyoutItem("Settings",nameof(SettingsView)).SetIcon(MaterialDesignThemes.Wpf.PackIconKind.Settings));

            
            ApiClient c = null;
            try
            {
                c = await ApiClient.GetInstance("", "");
            }
            catch { }
           
            if (c is null)
            {
                LoginWindow w = new();
                w.ShowDialog();

                if (w.Result == null)
                    App.Current.Shutdown();
            }

            MainWindow.Show();

            await Shell.Current.GoToAsync(nameof(HomeView), false);

            base.OnStartup(e);  
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
            base.OnExit(e);
        }
    }

    public static class AppExtensions
    {
        public static void Restart(this Application app)
        {
            app.Shutdown();
            System.Diagnostics.Process.Start(Environment.GetCommandLineArgs()[0]);
        }
    }
}
