using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Exodus.Models;
using Exodus.Services;
using Exodus.Views;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Exodus.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        this.WhenAnyValue(x => x.TotalBalanceInUsdPlayer).Subscribe(btcBalanceInUsd =>
        {
            TotalBalanceInUsdPlayerString = btcBalanceInUsd.ToString("N");
        });

        TransactionService.Instance.AddTransactionEvent += (sender, args) => AddTransaction();
        TransactionService.Instance.StopPendingEvent += (sender, args) =>
        {
            SelectedCoin!.ViewPart!.ForceStopPending = true;
        };
        TransactionService.Instance.DeleteLastTransactionEvent += (sender, args) => DeleteLastTransaction();
        TransactionService.Instance.ResetEvent += (sender, args) => Reset();
        TransactionService.Instance.TransactionAddedEvent += (sender, args) => new Thread(PlayNotification).Start();

        Coins = DataBaseService.Instance.Preferences!.Coins!;
        
        InitializeCoins();
        SetupViewPart();

        SelectedCoinIndex = 0;

        CoinManagementService.Instance.CoinSelectedEvent += (sender, args) =>
        {
            SelectedCoinIndex = Coins.IndexOf((Coin)sender!);
        };

        TransactionService.Instance.PriceUpdated += (sender, args) =>
        {
            Coins[0].ViewPart!.CalculateBalance(false);
            Coins[1].ViewPart!.CalculateBalance(false);
        };

        this.WhenAnyValue(x => x.SelectedCoinIndex).Subscribe(btcBalanceInUsd =>
        {
            if (SelectedCoin != null) SelectedCoin.IsSelected = false;

            SelectedCoin = Coins?[SelectedCoinIndex]!;
            SelectedCoinViewPart = SelectedCoin.ViewPart!;

            SelectedCoin.IsSelected = true;
        });

        NotificationViewModel = new NotificationViewModel()
        {
            NotificationStripeWidth = 360,
            IsNotification = false,
            IsShow = false
        };

        ReceiveAddressViewModel = new ReceiveAddressViewModel(this)
        {
            IsShow = false,
        };

        RefreshViewModel = new RefreshViewModel(this)
        {
            IsShow = false,
        };

        DataBaseService.Instance.IsInitialized = true;
    }

    private void InitializeCoins()
    {
        var i = 0;
        var l = Coins.Count;
        while (i < l)
        {
            Coins[i].Initialize();
            i++;
        }
    }

    private void SetupViewPart()
    {
        var bitcoinViewPart = new CoinSpecificPartViewModel(this, Coins[0])
        {
            LogoUri = new Uri("avares://Exodus/Assets/Resources/Icons/bitcoin.png"),
            TransactionLookUrl = "https://mempool.space/tx/",
            AddressLookUrl = "https://mempool.space/address/",
            Foreground1 = SolidColorBrush.Parse("#FFC82D"),
            Foreground2 = new LinearGradientBrush()
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops()
                {
                    new(Color.Parse("#E6FFC82D"), 0),
                    new(Color.Parse("#B3FFFC2D"), 1)
                }
            },
            Transactions = Coins[0].Transactions,
            IsThereTransaction = Coins[0].Transactions.Count > 0,
            IsThereAnyTransaction = Coins[0].Transactions.Count > 0
        };

        var ethereumViewPart = new CoinSpecificPartViewModel(this, Coins[1])
        {
            LogoUri = new Uri("avares://Exodus/Assets/Resources/Icons/ethereum_1.png"),
            TransactionLookUrl = "https://etherscan.io/tx/",
            AddressLookUrl = "https://etherscan.io/address/",
            Foreground1 = SolidColorBrush.Parse("#8C93AF"),
            Foreground2 = new LinearGradientBrush()
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops()
                {
                    new(Color.Parse("#E68C93AF"), 0),
                    new(Color.Parse("#B38D8CAF"), 1)
                }
            },
            Transactions = Coins[1].Transactions,
            IsThereTransaction = Coins[1].Transactions.Count > 0,
            IsThereAnyTransaction = Coins[1].Transactions.Count > 0
        };

        Coins[0].ViewPart = bitcoinViewPart;
        Coins[1].ViewPart = ethereumViewPart;

        var i = 0;
        while (i < 2)
        {
            Coins[i].ViewPart!.CalculateBalance(false);
            Coins[i].ViewPart!.AdjustTransactionsGroup();
            i++;
        }
    }

    [Reactive] public ObservableCollection<Coin> Coins { get; set; }

    [Reactive] public int SelectedCoinIndex { get; set; }

    [Reactive] public Coin SelectedCoin { get; set; }

    [Reactive] public CoinSpecificPartViewModel SelectedCoinViewPart { get; set; }

    [Reactive] public NotificationViewModel NotificationViewModel { get; set; }

    [Reactive] public ReceiveAddressViewModel ReceiveAddressViewModel { get; set; }

    [Reactive] public RefreshViewModel RefreshViewModel { get; set; }

    [Reactive] public double TotalBalanceInUsd { get; set; }

    [Reactive] public double TotalBalanceInUsdPlayer { get; set; }

    [Reactive] public string TotalBalanceInUsdPlayerString { get; set; }

    [Reactive] public double CurrentlyAdded { get; set; }

    public async void AddTransaction()
    {
        if (!SelectedCoinViewPart.IsAddedToOnWaitTransaction)
        {
            var window = new AddTransactionWindow();
            var viewModel = new AddTransactionWindowViewModel(window, SelectedCoin, SelectedCoinIndex);

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
            window.DataContext = viewModel;
            await window.ShowDialog(desktop.MainWindow!);

            SelectedCoinViewPart.IsAddedToOnWaitTransaction = viewModel.SuccessfullyAdd;
            if (SelectedCoinViewPart.IsAddedToOnWaitTransaction)
            {
                SelectedCoinViewPart.OnWaitTransaction = viewModel.MyTransaction;
            }

            return;
        }

        SelectedCoinViewPart.PendingTransactions.Insert(0, SelectedCoinViewPart.OnWaitTransaction);
        SelectedCoinViewPart.IsAddedToOnWaitTransaction = false;
        Task.Run(() =>
        {
            var currentId = SelectedCoin.Id;
            var transaction = SelectedCoinViewPart.OnWaitTransaction;
            var waiting = 0;
            while (waiting < 300)
            {
                if (SelectedCoinViewPart.ForceStopPending && currentId == SelectedCoin.Id)
                {
                    SelectedCoinViewPart.ForceStopPending = false;
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(950));
                waiting++;
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                SelectedCoinViewPart.PendingTransactions.Remove(transaction);
                SelectedCoinViewPart.Transactions.Insert(0, transaction);

                SelectedCoinViewPart.IsCurrentlyPending =
                    SelectedCoinViewPart.PendingTransactions.Count > 0;
                SelectedCoinViewPart.IsTherePendingTransaction =
                    SelectedCoinViewPart.PendingTransactions.Count > 0;
                SelectedCoinViewPart.IsThereTransaction = true;

                SelectedCoinViewPart.TransactionsChanged();
                SelectedCoinViewPart.CalculateBalance();
                SelectedCoinViewPart.AddTransactionToToday(transaction);

                DataBaseService.Instance.UpdatePref();
            });
        });
        CurrentlyAdded = SelectedCoinViewPart.OnWaitTransaction.Amount;
        SelectedCoinViewPart.IsCurrentlyPending = true;
        TransactionService.Instance.TransactionAdded();
    }

    public async void DeleteLastTransaction()
    {
        if (SelectedCoinViewPart.Transactions.Count > 0)
        {
            SelectedCoinViewPart.Transactions.RemoveAt(0);
            SelectedCoinViewPart.TransactionsChanged();
            SelectedCoinViewPart.AdjustTransactionsGroup();

            DataBaseService.Instance.UpdatePref();
        }
    }

    public async void Reset()
    {
        Coins = new ObservableCollection<Coin>(DataBaseService.Instance.GetEmptyCoins());
        SetupViewPart();
        InitializeCoins();
        SelectedCoinIndex = Coins.IndexOf(SelectedCoin) == -1 ? 0 : Coins.IndexOf(SelectedCoin);
        
        DataBaseService.Instance.Preferences!.Coins = Coins;
        DataBaseService.Instance.UpdatePref();
    }

    private void ReorderCoins()
    {
        Coins = new ObservableCollection<Coin>(Coins.OrderByDescending(c => c.ViewPart != null)
            .ThenByDescending(c => c.ViewPart?.BalanceInUsd));
        SelectedCoinIndex = Coins.IndexOf(SelectedCoin) == -1 ? 0 : Coins.IndexOf(SelectedCoin);
    }

    private bool _isBalanceAdderBusy = false;

    public async void BalanceAdder(bool isDoAnimation = true)
    {
        while (true)
        {
            if (_isBalanceAdderBusy) continue;
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            break;
        }

        TotalBalanceInUsd = Coins.Sum(c =>
        {
            if (c.ViewPart != null) return c.ViewPart.BalanceInUsd;
            ReorderCoins();
            return 0;
        });

        _isBalanceAdderBusy = true;
        if (TotalBalanceInUsd == 0)
        {
            Dispatcher.UIThread.InvokeAsync(() => TotalBalanceInUsdPlayer = 0);
            _isBalanceAdderBusy = false;
            ReorderCoins();
            return;
        }

        if (isDoAnimation)
        {
            var isAdd = TotalBalanceInUsdPlayer < TotalBalanceInUsd;
            var rand = new Random();
            while (isAdd ? TotalBalanceInUsdPlayer < TotalBalanceInUsd : TotalBalanceInUsdPlayer > TotalBalanceInUsd)
            {
                var toAdd = (isAdd ? 475 : -475) + (double)rand.Next(1, 99) / 100;
                if (Math.Abs(TotalBalanceInUsd - TotalBalanceInUsdPlayer) < Math.Abs(toAdd))
                    toAdd = TotalBalanceInUsd - TotalBalanceInUsdPlayer;

                Dispatcher.UIThread.InvokeAsync(() => TotalBalanceInUsdPlayer += toAdd);
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => TotalBalanceInUsdPlayer = TotalBalanceInUsd);
        }

        _isBalanceAdderBusy = false;
        ReorderCoins();
    }

    private void PlayNotification()
    {
        void PlayThread()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            var audioStream = AssetLoader.Open(new Uri($"avares://Exodus/Assets/Resources/Audios/receive.wav"));
            audioStream.Seek(0, SeekOrigin.Begin);
            audioStream.Position = 0;
            var player = new SoundPlayer(audioStream);
            player.Play();
        }

        new Thread(PlayThread).Start();

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            NotificationViewModel = new NotificationViewModel
            {
                Logo = new Bitmap(AssetLoader.Open(SelectedCoinViewPart.LogoUri)),
                IsNotification = true,
                IsShow = true,
                ViewPart = SelectedCoinViewPart,
                CurrentlyAdded = CurrentlyAdded,
                NotificationStripeWidth = 360
            };
        });

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(360));

            NotificationViewModel.NotificationStripeWidth = 0;

            await Task.Delay(TimeSpan.FromMilliseconds(3700));

            NotificationViewModel.IsShow = false;
            await Task.Delay(TimeSpan.FromMilliseconds(360));
            NotificationViewModel.IsNotification = false;

            NotificationViewModel.NotificationStripeWidth = 360;
        });
    }
}