using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmHelpers;
using OverAudible.API;
using OverAudible.Commands;
using OverAudible.DownloadQueue;
using OverAudible.EventMessages;
using OverAudible.Models;
using OverAudible.Services;
using OverAudible.Views;
using ShellUI.Attributes;
using ShellUI.Controls;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Singleton)]
    [QueryProperty("UseOfflineMode", "UseOfflineMode")]
    public partial class LibraryViewModel : ViewModelBase
    {
        private readonly LibraryService _libraryService;
        private readonly IDataService<Item> _dataService;

        public ConcurrentObservableCollection<Item> Library { get; set; }
        public ConcurrentObservableCollection<Item> Wishlist { get; set; }
        public ConcurrentObservableCollection<Collection> Collections { get; set; }

        public StandardCommands StandardCommands { get; }

        private int currentPage = 1;

        public bool IsPlayingSample { get; set; } = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DontUseOfflineMode))]
        bool useOfflineMode;

        public bool DontUseOfflineMode => !UseOfflineMode;

        public LibraryViewModel(LibraryService libraryService, StandardCommands standardCommands, IDownloadQueue download, IDataService<Item> dataService)
        {
            _libraryService = libraryService;
            StandardCommands = standardCommands;
            Library = new();
            Wishlist = new();
            Collections = new();
            Shell.Current.EventAggregator.Subscribe<RefreshLibraryMessage>(OnLibraryRefreshMessageReceived);
            download.ProgressChanged += (pco) =>
            {
                Debug.WriteLine($"{pco.Asin} | {pco.Title} | pc: {pco.downloadProgress.ProgressPercentage} |  br: {pco.downloadProgress.BytesReceived} | tbr: {pco.downloadProgress.TotalBytesToReceive}");
            };
            _dataService = dataService;
        }

        private void OnLibraryRefreshMessageReceived(RefreshLibraryMessage obj)
        {
            if (obj.InnerMessage is NewCollectionMessage msg)
            {
                Collections.Add(msg.Collection);
            }

            if (obj.InnerMessage is WishlistModifiedMessage msg2)
            {
                if (msg2.Action == WishlistAction.Added)
                    Wishlist.Add(msg2.Item);
                if (msg2.Action == WishlistAction.Removed)
                    Wishlist.Remove(msg2.Item);
            }    
        }

        [RelayCommand]
        async Task CreateCollection()
        {
            await Shell.Current.ModalGoToAsync(nameof(NewCollectionModal));
        }

        [RelayCommand]
        async Task CollectionOptions((string, string) nameAndID)
        {
            string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync(nameAndID.Item1, "Cancel", null, "Delete");

            if (result == null)
                return;

            if (result == "Delete")
            {
                var api = await ApiClient.GetInstance();

                await api.DeleteCollectionAsync(nameAndID.Item2);

                _libraryService.RemoveCollection(nameAndID.Item2);

                OnCollectionRemoved(nameAndID.Item2);
            }
        }

        [RelayCommand]
        void Sample(Item item)
        {
            if (IsPlayingSample)
            {
                StandardCommands.StopSampleCommand.Execute(null);
                IsPlayingSample = false;
            }
            else
            {
                IsPlayingSample = true;
                StandardCommands.PlaySampleCommand.Execute(item);
            }
        }

        public async Task LoadAsync()
        {
            if (UseOfflineMode)
            {
                var l = await _dataService.GetAll();

                foreach (var item in l)
                {
                    item.ProductImages.The500 = new Uri(Constants.DownloadFolder + $@"\{item.Asin}\{item.Asin}_Cover.jpg");
                }

                Library.AddRange(l);
            }

            else
            {
                if (IsBusy)
                    return;

                try
                {
                    IsBusy = true;

                    var l = await _libraryService.GetLibraryAsync();
                    var w = await _libraryService.GetWishlistAsync();
                    var c = await _libraryService.GetCollectionsAsync();

                    if (Library.Count > 0)
                        Library.Clear();
                    Library.AddRange(l);

                    if (Wishlist.Count > 0)
                        Wishlist.Clear();
                    Wishlist.AddRange(w);

                    if (Collections.Count > 0)
                        Collections.Clear();
                    Collections.AddRange(c);

                    foreach (Collection col in Collections)
                    {
                        if (col.BookAsins.Count == 1)
                        {
                            col.Image1 = Library.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                        }
                        if (col.BookAsins.Count == 2)
                        {
                            col.Image1 = Library.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                            col.Image2 = Library.First(x => x.Asin == col.BookAsins[1]).ProductImages.The500.AbsoluteUri;
                        }
                        if (col.BookAsins.Count == 3)
                        {
                            col.Image1 = Library.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                            col.Image2 = Library.First(x => x.Asin == col.BookAsins[1]).ProductImages.The500.AbsoluteUri;
                            col.Image3 = Library.First(x => x.Asin == col.BookAsins[2]).ProductImages.The500.AbsoluteUri;
                        }
                        if (col.BookAsins.Count == 4)
                        {
                            col.Image1 = Library.First(x => x.Asin == col.BookAsins[0]).ProductImages.The500.AbsoluteUri;
                            col.Image2 = Library.First(x => x.Asin == col.BookAsins[1]).ProductImages.The500.AbsoluteUri;
                            col.Image3 = Library.First(x => x.Asin == col.BookAsins[2]).ProductImages.The500.AbsoluteUri;
                            col.Image4 = Library.First(x => x.Asin == col.BookAsins[3]).ProductImages.The500.AbsoluteUri;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }
                finally
                {
                    IsBusy = false;
                }
            }

        }

        private void OnCollectionRemoved(string collectionID)
        {
            Collections.Remove(Collections.First(x => x.CollectionId == collectionID));
        }
    }
}
