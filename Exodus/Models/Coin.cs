using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using Exodus.Services;
using Exodus.ViewModels;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.Models;

public class Coin : ReactiveObject
{
    public void Initialize()
    {
        // for solana (id 4)
        IsShowPercentageIcon = Id == 4;
    }
    
    [JsonProperty] [Reactive] public int Id { get; set; }
    
    [JsonProperty] [Reactive] public string Name { get; set; }
    [JsonProperty] [Reactive] public string Symbol { get; set; }
    
    [JsonProperty] [Reactive] public string Address { get; set; }
    
    [JsonProperty] [Reactive] public string StartValue { get; set; }
    [JsonProperty] [Reactive] public double Balance { get; set; } = 0.00f;
    
    [JsonProperty] [Reactive] public ObservableCollection<Transaction> Transactions { get; set; } = new();
    
    [JsonProperty] [Reactive] public bool IsEmpty { get; set; }
    
    [Reactive] public bool IsSelected { get; set; }
    
    [Reactive] public bool IsShowPercentageIcon { get; set; }
    
    [Reactive] public CoinSpecificPartViewModel? ViewPart { get; set; }

    public void SelectedCommand()
    {
        if (Id > 1) return;
        CoinManagementService.Instance.CoinSelected(this);
    }
}