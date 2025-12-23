using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Exodus.ViewModels;

namespace Exodus.Views;

public partial class CoinSpecificPartView : Border
{
    public CoinSpecificPartView()
    {
        InitializeComponent();

    }

    private void LogoButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        (DataContext as CoinSpecificPartViewModel)!.IsShowRefresh = true;
    }

    private void LogoButton_OnPointerExited(object? sender, PointerEventArgs e)
    {
        (DataContext as CoinSpecificPartViewModel)!.IsShowRefresh = false;
    }
}