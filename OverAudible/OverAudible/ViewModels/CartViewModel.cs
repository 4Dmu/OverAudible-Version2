﻿using CommunityToolkit.Mvvm.ComponentModel;
using OverAudible.Services;
using ShellUI.Attributes;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models;
using ShellUI.Controls;
using OverAudible.EventMessages;
using CommunityToolkit.Mvvm.Input;
using OverAudible.Commands;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public partial class CartViewModel : ViewModelBase
    {
        private readonly CartService _cartService;
        private StandardCommands _commands;

        [ObservableProperty]
        decimal subTotal;

        public ConcurrentObservableCollection<Item> Cart { get; private set; }

        public CartViewModel(CartService cart, StandardCommands commands)
        {
            _cartService = cart;
            _commands = commands;
            Cart = new();
            Cart.AddRange(_cartService.GetCart());
            var v = Cart.Select(x => x.Price.LowestPrice.Base).Sum();
            SubTotal = v is null ? 0.0m : (decimal)v;
            Shell.Current.EventAggregator.Subscribe<RefreshCartMessage>(OnRefreshCartMessageReceived);
            Cart.CollectionChanged += (s, e) =>
            {
                var v = Cart.Select(x => x.Price.LowestPrice.Base).Sum();
                SubTotal = v is null ? 0.0m : (decimal)v;
            };
        }

        private void OnRefreshCartMessageReceived(RefreshCartMessage obj)
        {
            if (obj.InnerMessage is ItemAddedToCartMessage msg)
            {
                Cart.Add(msg.AddedItem);
            }
        }

        [RelayCommand]
        async Task RemoveFromCart(Item item)
        {
            Cart.Remove(item);
            _cartService.RemoveCartItem(item);
        }

        [RelayCommand]
        async Task RemoveFromCartAndAddToWishlist(Item item)
        {
            await RemoveFromCart(item);
            _commands.AddToWishlistCommand.Execute(item);
        }
    }
}
