﻿using ShellUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using ShellUI.Attributes;
using System.Windows.Media;
using OverAudible.Services;
using ShellUI.Controls;
using OverAudible.EventMessages;
using OverAudible.Helpers;
using OverAudible.API;

namespace OverAudible.Commands
{
    [Inject(InjectionType.Transient)]
    public class AddToCartCommand : AsyncCommandBase
    {
        private readonly CartService _cartService;

        public AddToCartCommand(CartService cartService)
        {
            _cartService = cartService;
        }

        public override async Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                if (_cartService.GetCart().Any(x => x.Asin == item.Asin))
                    return;

                item = item.IsInLibrary ? await item.GetSelfFromLibrary(AudibleApi.LibraryOptions.ResponseGroupOptions.ALL_OPTIONS) : await item.GetSelfFromCatalog(AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
                _cartService.AddCartItem(item);
                Shell.Current.EventAggregator.Publish(new RefreshCartMessage(new ItemAddedToCartMessage(item)));
            }
        }
    }
    
    [Inject(InjectionType.Transient)]
    public class AddToWishlistCommand : AsyncCommandBase
    {
        private readonly LibraryService _libraryService;

        public AddToWishlistCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                var api = await ApiClient.GetInstance();

                if (await api.Api.IsInWishListAsync(item.Asin))
                    return;

                await api.Api.AddToWishListAsync(item.Asin);

                _libraryService.AddItemToWishlist(item);

                Shell.Current.EventAggregator.Publish<RefreshLibraryMessage>(new RefreshLibraryMessage(new WishlistModifiedMessage(item,WishlistAction.Added)));
            }
        }
    }

    [Inject(InjectionType.Transient)]
    public class RemoveFromWishlistCommand : AsyncCommandBase
    {
        private readonly LibraryService _libraryService;

        public RemoveFromWishlistCommand(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                var api = await ApiClient.GetInstance();

                await api.Api.DeleteFromWishListAsync(item.Asin);

                _libraryService.DeleteItemFromWishlist(item);

                Shell.Current.EventAggregator.Publish<RefreshLibraryMessage>(new RefreshLibraryMessage(new WishlistModifiedMessage(item,WishlistAction.Removed)));
            }
        }
    }

    [Inject(InjectionType.Transient)]
    public class PlayCommand : AsyncCommandBase
    {
        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {

            }
        }
    }

    [Inject(InjectionType.Transient)]
    public class DownloadCommand : AsyncCommandBase
    {
        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {

            }
        }
    }

    [Inject(InjectionType.Transient)]
    public class PlaySampleCommand : AsyncCommandBase
    {
        private readonly MediaPlayer _mediaPlayer;

        public PlaySampleCommand(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {
                _mediaPlayer.Open(item.SampleUrl);
                _mediaPlayer.Play();
            }
        }
    }

    [Inject(InjectionType.Transient)]
    public class StopSampleCommand : AsyncCommandBase
    {
        private readonly MediaPlayer _mediaPlayer;

        public StopSampleCommand(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public async override Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);
            _mediaPlayer.Pause();
            _mediaPlayer.Close();
        }
    }

    [Inject(InjectionType.Transient)]
    public class StandardCommands
    {
        public AddToCartCommand AddToCartCommand { get; }
        public AddToWishlistCommand AddToWishlistCommand { get; }
        public RemoveFromWishlistCommand RemoveFromWishlistCommand { get; }
        public PlayCommand PlayCommand { get; }
        public DownloadCommand DownloadCommand { get; }
        public PlaySampleCommand PlaySampleCommand { get; }
        public StopSampleCommand StopSampleCommand { get; }

        public StandardCommands(AddToCartCommand addToCartCommand, 
            AddToWishlistCommand addToWishlistCommand, 
            RemoveFromWishlistCommand removeFromWishlistCommand, 
            PlayCommand playCommand, DownloadCommand downloadCommand, 
            PlaySampleCommand playSampleCommand, 
            StopSampleCommand stopSampleCommand)
        {
            AddToCartCommand = addToCartCommand;
            AddToWishlistCommand = addToWishlistCommand;
            RemoveFromWishlistCommand = removeFromWishlistCommand;
            PlayCommand = playCommand;
            DownloadCommand = downloadCommand;
            PlaySampleCommand = playSampleCommand;
            StopSampleCommand = stopSampleCommand;
        }
    }


}
