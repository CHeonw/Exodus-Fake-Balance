using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.ViewModels;

public class RefreshViewModel : ViewModelBase
{
    public RefreshViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowVm = mainWindowViewModel;
        
        RefreshText = $"Are you sure you want to rescan the {MainWindowVm.SelectedCoin.Name} blockchain?";
        this.WhenAnyValue(x => x.MainWindowVm!.SelectedCoin)
            .Subscribe(_ =>
            {
                RefreshText = $"Are you sure you want to rescan the {MainWindowVm.SelectedCoin.Name} blockchain?";
            });
        
        IsShow = false;
        IsShowButtons = false;

        Opacity = 0;
        SolidOpacity = 0;

        this.WhenAnyValue(x => x.IsShow)
            .Subscribe(_ =>
            {
                if (IsShow) IsVisible = true;
                Opacity = IsShow ? 0.974 : 0;
                SolidOpacity = IsShow ? 1 : 0;

                if (IsShow)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(600);
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            IsShowButtons = true;
                            ButtonsOpacity = 1;
                        });
                    });
                    return;
                }

                IsShowButtons = false;
                ButtonsOpacity = 0;
                
                Task.Run(async () =>
                {
                    await Task.Delay(355);
                    Dispatcher.UIThread.InvokeAsync(() => IsVisible = false);
                });
            });
    }

    [Reactive] public MainWindowViewModel? MainWindowVm { get; set; }
    
    [Reactive] public string RefreshText { get; set; }
    
    [Reactive] public double SolidOpacity { get; set; }
    
    [Reactive] public double Opacity { get; set; }

    [Reactive] public bool IsShow { get; set; }
    
    [Reactive] public bool IsShowButtons { get; set; }
    
    [Reactive] public double ButtonsOpacity { get; set; }

    [Reactive] public bool IsVisible { get; set; }

    public async void YesRefreshCommand()
    {
        MainWindowVm!.SelectedCoinViewPart.DoRefreshCommand();
        CloseCommand();
    }

    public void CloseCommand()
    {
        IsShow = false;
    }
}