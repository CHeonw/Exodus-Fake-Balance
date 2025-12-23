using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Threading;
using Exodus.Models;
using Newtonsoft.Json;

namespace Exodus.Services;

public class DataBaseService
{
    public static DataBaseService Instance { get; } = new();

    private DataBaseService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(appDataPath, "ExodusModded");
        AppDataPath = path;
        Directory.CreateDirectory(path);
        Prefpath = Path.Combine(path, "pref.json");

        if (File.Exists(Prefpath))
        {
            var json = File.ReadAllText(Prefpath);
            Preferences = JsonConvert.DeserializeObject<PrefModel>(json)!;
            if (Preferences == null)
            {
                File.Delete(Prefpath);
                Preferences = new PrefModel
                {
                    Coins = new ObservableCollection<Coin>(GetEmptyCoins())
                };
                UpdatePref();
            }
            else
            {
                Preferences.Coins ??= new ObservableCollection<Coin>(GetEmptyCoins());
                var i = 0;
                while (i < 2)
                {
                    if (string.IsNullOrEmpty(Preferences.Coins[i].Address))
                        Preferences.Coins[i].Address =
                            $"{Preferences.Coins[i].StartValue}{TransactionService.Instance.RandomString(39)}";
                    i++;
                }
                UpdatePref();
            }
        }
        else
        {
            Preferences = new PrefModel
            {
                Coins = new ObservableCollection<Coin>(GetEmptyCoins())
            };
            UpdatePref();
        }

        foreach (var coin in Preferences.Coins)
        {
            foreach (var transaction in coin.Transactions)
            {
                transaction.SetConst(transaction.Amount, coin, true);
            }
        }
    }

    private string AppDataPath { get; }
    private string Prefpath { get; }
    public PrefModel? Preferences { get; set; }

    public bool IsInitialized { get; set; } = false;

    /// <summary>
    ///     A preference updater
    /// </summary>
    public void UpdatePref()
    {
        var json = JsonConvert.SerializeObject(Preferences, Formatting.Indented);
        Dispatcher.UIThread.InvokeAsync(() => File.WriteAllText(Prefpath, json));
    }

    public List<Coin> GetEmptyCoins()
    {
        return new List<Coin>()
        {
            new Coin()
            {
                Id = 0, Name = "Bitcoin", Symbol = "BTC", StartValue = "bc1",
                Address = $"bc1{TransactionService.Instance.RandomString(39)}"
            },
            new Coin()
            {
                Id = 1, Name = "Ethereum", Symbol = "ETH", StartValue = "0x",
                Address = $"0x{TransactionService.Instance.RandomString(39)}"
            },
            new Coin() { Id = 2, Name = "Litecoin", Symbol = "LTC" },
            new Coin() { Id = 3, Name = "Dogecoin", Symbol = "DOGE" },
            new Coin() { Id = 4, Name = "Solana", Symbol = "SOL" },
            new Coin() { Id = 5, Name = "Cardano", Symbol = "ADA" },
            new Coin() { Id = 6, Name = "Polkadot", Symbol = "DOT" },
            new Coin() { Id = 7, Name = "Bitcoin Cash", Symbol = "BCH" },
            new Coin() { Id = 8, Name = "Chainlink", Symbol = "LINK" },
            new Coin() { Id = 9, Name = "Stellar", Symbol = "XLM" },
            new Coin() { Id = 10, Name = "Binance Coin", Symbol = "BNB" },
            new Coin() { Id = 11, Name = "Tether", Symbol = "USDT" },
            new Coin() { Id = 12, Name = "Monero", Symbol = "XMR" },
            new Coin() { Id = 13, Name = "TRON", Symbol = "TRX" },
        };
    }
}