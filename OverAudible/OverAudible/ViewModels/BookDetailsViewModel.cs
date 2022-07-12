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
using OverAudible.Commands;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    [QueryProperty("Item", "ItemParam")]
    public partial class BookDetailsViewModel : ViewModelBase
    {
        public StandardCommands StandardCommands { get; }

        [ObservableProperty]
        Item item;

        public BookDetailsViewModel(StandardCommands commands)
        {
            StandardCommands = commands;
        }


        [RelayCommand]
        void AddToCollection(Item item)
        {

        }
        

        [RelayCommand]
        async Task MoreOptions()
        {
            if (Item.IsInLibrary)
            {
                if (Item.ActualIsDownloaded)
                {
                    string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null,
                    "Play",
                    "Delete",
                    "Add to Collection",
                    "Write a review");
                }
                else
                {
                    string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null,
                   "Download",
                   "Add to Collection",
                   "Write a review");
                }
            }
            else
            {
                string result = await Shell.Current.CurrentPage.DisplayActionSheetAsync("More Options", "Cancel", null,
                   "Add to cart",
                   "Add to Wishlist");
            }
           
        }

        

    }
}
