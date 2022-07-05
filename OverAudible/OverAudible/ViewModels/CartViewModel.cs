using CommunityToolkit.Mvvm.ComponentModel;
using ShellUI.Attributes;
using ShellUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.ViewModels
{
    [Inject(InjectionType.Transient)]
    public partial class CartViewModel : ViewModelBase
    {
        [ObservableProperty]
        decimal subTotal;

        [ObservableProperty]
        int itemsCount;
    }
}
