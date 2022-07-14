using CommunityToolkit.Mvvm.ComponentModel;
using ShellUI.Attributes;
using OverAudible.Models;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Services;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("Collection", "CollectionParam")]
    public partial class CollectionDetailsViewModel : ViewModelBase
    {
        private readonly LibraryService _libraryService;
        private const int bookCount = 25;
        private const int bookCardHeightValue = 30;

        [ObservableProperty]
        Collection collection;

        public ConcurrentObservableCollection<Item>? Books { get; private set; } = null;

        public List<Item> TotalBooks { get; set; }

        public CollectionDetailsViewModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
            this.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(Collection) && Books == null)
                {
                    var library = await _libraryService.GetLibraryAsync();
                    Books = new();
                    TotalBooks = new();
                    TotalBooks.AddRange(library.Where(x => this.Collection.BookAsins.Any(y => y == x.Asin)));
                    Books.AddRange(TotalBooks.Count > bookCount ? TotalBooks.GetRange(0, bookCount) : TotalBooks);
                }
            };
        }

        [RelayCommand]
        void BooksScroll(RoutedEventArgs args)
        {
            if (IsBusy)
                return;
            if (args.Source is ScrollViewer sv)
            {

                if (sv.VerticalOffset > sv.ScrollableHeight - bookCardHeightValue
                    && !Books.Contains(TotalBooks.Last()))
                {
                    var itemToAdd = TotalBooks[TotalBooks.IndexOf(Books.Last()) + 1];
                    var itemToRemove = TotalBooks[TotalBooks.IndexOf(Books.First())];

                    Books.Remove(itemToRemove);
                    Books.Add(itemToAdd);
                    sv.ScrollToVerticalOffset(sv.ScrollableHeight - bookCardHeightValue);
                }
                else if (sv.VerticalOffset < bookCardHeightValue
                        && !Books.Contains(TotalBooks.First()))
                {
                    var first = Books.First();
                    int index = TotalBooks.IndexOf(first);
                    index--;
                    var last = Books.Last();
                    int lindex = TotalBooks.IndexOf(last);
                    var itemToAdd = TotalBooks[index];
                    var itemToRemove = TotalBooks[lindex];

                    Books.Remove(itemToRemove);
                    Books.Insert(0, itemToAdd);
                    sv.ScrollToVerticalOffset(bookCardHeightValue);
                }
            }
        }
    }
}
