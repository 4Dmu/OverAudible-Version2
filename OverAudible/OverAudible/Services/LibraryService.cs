using OverAudible.API;
using ShellUI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;

namespace OverAudible.Services
{
    [Inject(InjectionType.Singleton)]
    public class LibraryService
    {
        private readonly Lazy<Task> initLazy;

        private List<Item> library { get; set; }
        private List<Item> wishlist { get; set; }
        private List<Collection> collections { get; set; }

        public LibraryService()
        {
            initLazy = new Lazy<Task>(InitAsync);
            library = new List<Item>();
            wishlist = new List<Item>();
            collections = new List<Collection>();
        }

        private async Task InitAsync()
        {
            ApiClient apiClient = await ApiClient.GetInstance();

            library = await apiClient.GetLibraryAsync();
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
