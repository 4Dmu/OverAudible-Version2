using ShellUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using ShellUI.Attributes;
using System.Windows.Media;

namespace OverAudible.Commands
{
    [Inject(InjectionType.Transient)]
    public class AddToCartCommand : AsyncCommandBase
    {
        public override async Task ExecuteAsync(object paramater)
        {
            await Task.Delay(1);

            if (paramater is Item item)
            {

            }
        }
    }
    
    [Inject(InjectionType.Transient)]
    public class AddToWishlistCommand : AsyncCommandBase
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
    public class RemoveFromWishlistCommand : AsyncCommandBase
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
