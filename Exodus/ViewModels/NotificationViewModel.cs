using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.ViewModels;

public class NotificationViewModel : ViewModelBase
{
    public NotificationViewModel()
    {
        CloseNotification = ReactiveCommand.Create(_closeNotification);
    }
    [Reactive] public Bitmap Logo { get; set; }

    [Reactive] public bool IsNotification { get; set; }
    
    [Reactive] public bool IsShow { get; set; }
    
    [Reactive] public CoinSpecificPartViewModel? ViewPart { get; set; }

    [Reactive] public double CurrentlyAdded { get; set; }

    [Reactive] public double NotificationStripeWidth { get; set; }

    [Reactive] public ReactiveCommand<Unit, Unit> CloseNotification { get; set; }

    public async void _closeNotification()
    {
        IsShow = false;
        await Task.Delay(TimeSpan.FromMilliseconds(360));
        IsNotification = false;
            
        NotificationStripeWidth = 360;
    }
}