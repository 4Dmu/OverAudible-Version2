using CommunityToolkit.Mvvm.ComponentModel;
using ShellUI.Attributes;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShellUI.Controls;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("Item", "ItemParam")]
    public partial class BookDetailsViewModel : ViewModelBase
    {
        [ObservableProperty]
        Item item;

        public BookDetailsViewModel()
        {
            
        }

        [RelayCommand]
        void AddToWishlist()
        {

        }

        [RelayCommand]
        void RemoveFromWishlist()
        {

        }

        [RelayCommand]
        void AddToCollection()
        {

        }

        [RelayCommand]
        void Play()
        {
            
        }

        [RelayCommand]
        async Task MoreOptions()
        {
            string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null, 
                "Purchase", 
                "Add to Wishlist", 
                "Write a review");
        }

        [RelayCommand]
        void Purchase()
        {

        }

    }
}
