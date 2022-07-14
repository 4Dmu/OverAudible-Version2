using OverAudible.API;
using ShellUI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using ShellUI.Controls;
using OverAudible.Windows;
using OverAudible.EventMessages;

namespace OverAudible.Services
{
    [Inject(InjectionType.Singleton)]
    public class LibraryService
    {
        private readonly Lazy<Task> initLazy;
        private readonly LibraryDataService _libraryDataService;

        private List<Item> library { get; set; }
        private List<Item> wishlist { get; set; }
        private List<Collection> collections { get; set; }

        public LibraryService(LibraryDataService libraryDataService)
        {
            _libraryDataService = libraryDataService;
            initLazy = new Lazy<Task>(InitAsync);
            library = new List<Item>();
            wishlist = new List<Item>();
            collections = new List<Collection>();
        }

        private async Task SyncLibraryAsync()
        {
            ApiClient apiClient = await ApiClient.GetInstance();
            var l = await apiClient.GetLibraryAsync();
            var dl = await _libraryDataService.GetAll();

            if (dl.Count >= l.Count)
            {
                return;
            }
                

            foreach (var item in l)
            {
                if (!dl.Contains(item))
                {
                    await _libraryDataService.Create(item);
                }
            }

            var lib = await _libraryDataService.GetAll();
            library = lib;

            Shell.Current.EventAggregator.Publish(
                new RefreshLibraryMessage(new LocalAndServerLibrarySyncedMessage()));

        }


        private async Task InitAsync()
        {
            ApiClient apiClient = await ApiClient.GetInstance();

            var l = await _libraryDataService.GetAll();
            if (l.Count == 0)
            {
                library = await ProgressDialog.ShowDialogAsync<List<Item>>("Importing library", "Importing library from server, " +
                    "as this is your first time it could take quite a while.", async () => await apiClient.GetLibraryAsync());
                foreach (var item in library)
                {
                    await _libraryDataService.Create(item);
                }
            }
            else
            {
                library = l;
                _  = SyncLibraryAsync();
            }

            wishlist = await apiClient.GetWishlistAsync();
            collections = await apiClient.GetCollectionsWithItemsAsync();
        }

        public async Task<List<Item>> GetLibraryAsync()
        {
            await initLazy.Value;

            return library;
        }

        public async Task<List<Item>> GetWishlistAsync()
        {
            await initLazy.Value;

            return wishlist;
        }

        public async Task<List<Collection>> GetCollectionsAsync()
        {
            await initLazy.Value;

            return collections;
        }

        public void RemoveCollection(string id)
        {
            collections.Remove(collections.First(x => x.CollectionId == id));
        }

        public void AddCollection(Collection c)
        {
            collections.Add(c);
        }

        public void AddItemToWishlist(Item item)
        {
            wishlist.Add(item);
        }

        public void DeleteItemFromWishlist(Item item)
        {
            wishlist.Remove(item);
        }
    }
}
