using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using Exodus.Helpers;
using Exodus.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.ViewModels;

public class ReceiveAddressViewModel : ViewModelBase
{
    public ReceiveAddressViewModel(MainWindowViewModel mainWindowViewModel)
    {
        CopyCommand = ReactiveCommand.Create(_copyCommand);
        
        MainWindowVm = mainWindowViewModel;

        IsShow = false;

        Opacity = 0;
        SolidOpacity = 0;

        AddressOpacity = 1;
        CopyOpacity = 0;

        this.WhenAnyValue(x => x.IsShow)
            .Subscribe(_ =>
            {
                if (IsShow) IsVisible = true;
                Opacity = IsShow ? 0.974 : 0;
                SolidOpacity = IsShow ? 1 : 0;

                if (IsShow) return;
                Task.Run(async () =>
                {
                    await Task.Delay(355);
                    Dispatcher.UIThread.InvokeAsync(() => IsVisible = false);
                });
            });
    }

    [Reactive] public MainWindowViewModel? MainWindowVm { get; set; }

    [Reactive] public bool IsShow { get; set; }

    [Reactive] public bool IsVisible { get; set; }

    [Reactive] public double Opacity { get; set; }

    [Reactive] public double SolidOpacity { get; set; }

    [Reactive] public ReactiveCommand<Unit, Unit> CopyCommand { get; set; }

    [Reactive] public double CopyOpacity { get; set; }
    
    [Reactive] public double AddressOpacity { get; set; }
    
    public async void _copyCommand()
    {
        GeneralServices.Instance.CopyTextToClipBoard(MainWindowVm!.SelectedCoin.Address);
        
        CopyOpacity = 1;
        AddressOpacity = 0;
        await Task.Delay(TimeSpan.FromSeconds(1.05));
        CopyOpacity = 0;
        AddressOpacity = 1;
    }

    public async void ChainCommand()
    {
        var url = MainWindowVm.SelectedCoin.ViewPart!.AddressLookUrl + MainWindowVm.SelectedCoin.Address;
        UrlUtils.OpenUrl(url);
    }

    public void CloseCommand()
    {
        IsShow = false;
    }
}