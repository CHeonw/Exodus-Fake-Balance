using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Exodus.Services;

namespace Exodus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        AddHandler(KeyDownEvent, (sender, args) =>
        {
            switch (args)
            {
                case { Key: Key.A, KeyModifiers: KeyModifiers.Control }:
                    TransactionService.Instance.AddTransaction();
                    break;
                case { Key: Key.Z, KeyModifiers: KeyModifiers.Control }:
                    TransactionService.Instance.DeleteLastTransaction();
                    break;
                case { Key: Key.R, KeyModifiers: KeyModifiers.Control }:
                    TransactionService.Instance.Reset();
                    break;
                case { Key: Key.S, KeyModifiers: KeyModifiers.Control }:
                    TransactionService.Instance.StopPending();
                    break;
                case { Key: Key.X, KeyModifiers: KeyModifiers.Control }:
                    TransactionService.Instance.ChangeAddress();
                    break;
            }
        }, RoutingStrategies.Tunnel);

        Closing += (sender, args) =>
        {
            Environment.Exit(0);
        };
        
        GeneralServices.Instance.CopyTextToClipBoardEvent += (sender, args) =>
        {
            CopyToClipBoard(sender!.ToString()!);
        };
    }

    private async void CopyToClipBoard(string text)
    {
        var clipboard = GetTopLevel(this)?.Clipboard;
        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Text, text);
        await clipboard.SetDataObjectAsync(dataObject);
    }
}