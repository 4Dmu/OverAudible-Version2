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

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("Collection", "CollectionParam")]
    public partial class CollectionDetailsViewModel : ViewModelBase
    {
        private readonly LibraryService _libraryService;

        [ObservableProperty]
        Collection collection;

        public ConcurrentObservableCollection<Item>? Books { get; private set; } = null;

        public CollectionDetailsViewModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
            this.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(Collection) && Books == null)
                {
                    var library = await _libraryService.GetLibraryAsync();
                    Books = new();
                    Books.AddRange(library.Where(x => this.Collection.BookAsins.Any(y => y == x.Asin)));
                }
            };
        }
    }
}
