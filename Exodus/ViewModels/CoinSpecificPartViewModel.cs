using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Exodus.Models;
using Exodus.Services;
using Exodus.Views;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.ViewModels;

public class CoinSpecificPartViewModel : ViewModelBase
{
    public CoinSpecificPartViewModel()
    {
    }

    public MainWindowViewModel MainVm { get; set; }

    public CoinSpecificPartViewModel(MainWindowViewModel mainVm, Coin myCoin)
    {
        MainVm = mainVm;
        MyCoin = myCoin;

        IsBitcoin = myCoin.Id == 0;
        IsEthereum = myCoin.Id == 1;

        BannerControl = (DataTemplate)Application.Current.FindResource($"Banner{myCoin.Id}");
        BannerControlHeight = BannerControl.Build(null).Height;

        this.WhenAnyValue(x => x.BalanceInUsd).Subscribe(btcBalanceInUsd =>
        {
            BalanceInUsdString = btcBalanceInUsd.ToString("N");
        });

        this.WhenAnyValue(x => x.IsThereTransaction, x => x.IsTherePendingTransaction)
            .Subscribe(x => { IsThereAnyTransaction = IsThereTransaction || IsTherePendingTransaction; });

        Transactions
            .ToObservableChangeSet(x => x)
            .ToCollection()
            .Subscribe(x =>
            {
                CalculateBalance();
                IsThereTransaction = Transactions.Count > 0;
            });
        
        PendingTransactions
            .ToObservableChangeSet(x => x)
            .ToCollection()
            .Subscribe(x =>
            {
                CalculateBalance();
                IsTherePendingTransaction = PendingTransactions.Count > 0;
            });

        IsThereAnyTransaction = false;
        IsThereTransaction = false;
        IsTherePendingTransaction = false;

        TransactionService.Instance.ChangeAddressEvent += (sender, args) =>
        {
            if (MainVm.SelectedCoin.Id != MyCoin.Id) return;
            ChangeAddress();
        };
    }

    public void TransactionsChanged()
    {
        CalculateBalance();
        IsThereTransaction = Transactions.Count > 0;
        IsTherePendingTransaction = PendingTransactions.Count > 0;
    }
    
    public void AdjustTransactionsGroup()
    {
        var temp = new List<TransactionGroup>();
        var grouped = Transactions.GroupBy(t => DateTimeOffset.FromUnixTimeSeconds(t.Time).Date);
        foreach (var group in grouped)
        {
            var transactionGroup = new TransactionGroup(group.Key);
            var groupSorted = group.OrderBy(t => t.Time).Reverse();
            foreach (var transaction in groupSorted)
            {
                transactionGroup.Transactions!.Add(transaction);
            }

            temp.Add(transactionGroup);
        }

        if (temp.Count == 0) temp.Add(new TransactionGroup(DateTime.Today));

        temp = temp.OrderBy(t => t.Date).Reverse().ToList();
        TransactionsGroups = new ObservableCollection<TransactionGroup>(temp);
    }

    public void AddTransactionToToday(Transaction transaction)
    {
        TransactionsGroups[0].Transactions!.Insert(0, transaction);
    }

    public async void ReceiveCommand()
    {
        MainVm.ReceiveAddressViewModel.IsShow = true;
    }

    public async void RefreshCommand()
    {
        MainVm.RefreshViewModel.IsShow = true;
    }

    public void DoRefreshCommand()
    {
        if (IsShowRefresh) return;
        Task.Run(async() =>
        {
            Dispatcher.UIThread.InvokeAsync(() => IsShowRefresh = true);
            Dispatcher.UIThread.InvokeAsync(() => IsPlayRefresh = true);
            await Task.Delay(TimeSpan.FromSeconds(5));
            Dispatcher.UIThread.InvokeAsync(() => IsPlayRefresh = false);
            Dispatcher.UIThread.InvokeAsync(() => IsShowRefresh = false);
        });
    }

    [Reactive] public bool IsShowRefresh { get; set; }
    
    [Reactive] public bool IsPlayRefresh { get; set; }

    [Reactive] public bool IsBitcoin { get; set; }
    
    [Reactive] public bool IsEthereum { get; set; }

    [Reactive] public Coin MyCoin { get; set; }

    [Reactive] public Uri LogoUri { get; set; }
    
    [Reactive] public string TransactionLookUrl { get; set; }
    
    [Reactive] public string AddressLookUrl { get; set; }

    [Reactive] public DataTemplate? BannerControl { get; set; }

    [Reactive] public double BannerControlHeight { get; set; }

    [Reactive] public bool IsCurrentlyPending { get; set; }

    [Reactive] public double Balance { get; set; }

    [Reactive] public double BalanceInUsd { get; set; }
    [Reactive] public string BalanceInUsdString { get; set; }

    [Reactive] public bool IsTherePendingTransaction { get; set; }

    [Reactive] public string TotalInPending { get; set; }

    [Reactive] public bool IsThereAnyTransaction { get; set; }
    
    [Reactive] public bool ForceStopPending { get; set; }
    
    [Reactive] public Transaction OnWaitTransaction { get; set; }

    [Reactive] public bool IsAddedToOnWaitTransaction { get; set; }

    [Reactive] public ObservableCollection<Transaction> Transactions { get; set; } = new();

    [Reactive] public ObservableCollection<TransactionGroup> TransactionsGroups { get; set; } = new();

    [Reactive] public ObservableCollection<Transaction> PendingTransactions { get; set; } = new();

    [Reactive] public bool IsThereTransaction { get; set; }

    [Reactive] public SolidColorBrush Foreground1 { get; set; }

    [Reactive] public LinearGradientBrush Foreground2 { get; set; }


    public void CalculateBalance(bool isDoAnimation = true)
    {
        var total = new List<Transaction>();

        foreach (var transaction in Transactions)
        {
            if (total.FirstOrDefault(t => t.Id == transaction.Id) == null)
                total.Add(transaction);
        }

        var totalInPending = 0.00;
        foreach (var transaction in PendingTransactions)
        {
            totalInPending += transaction.Amount;
            if (total.FirstOrDefault(t => t.Id == transaction.Id) == null)
                total.Add(transaction);
        }

        TotalInPending = $"{totalInPending} {MyCoin.Symbol}";

        var balance = total.Sum(b => b.Amount);
        Balance = Math.Round(balance, 8);
        BalanceInUsd = Math.Round(Balance * TransactionService.Instance.CurrentCoinsPrice[MyCoin.Id], 2);
        MyCoin.Balance = Balance;
        MyCoin.IsEmpty = Balance == 0;
        Task.Run(() => MainVm.BalanceAdder(isDoAnimation));
    }

    private async void ChangeAddress()
    {
        var window = new UpdateAddressWindow();
        var viewModel = new UpdateAddressWindowViewModel(window, MyCoin.Address);
            
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        window.DataContext = viewModel;
        await window.ShowDialog(desktop.MainWindow!);

        if (!viewModel.SuccessfullyUpdated) return;
        MyCoin.Address = viewModel.NewAddress;
        DataBaseService.Instance.UpdatePref();
    }
}