using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Media;
using Exodus.Helpers;
using Exodus.Services;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.Models;

public class Transaction : ReactiveObject
{
    public Transaction()
    {
    }

    public void SetConst(double amount, Coin coin, bool isInitializedWithData = false)
    {
        MyCoin = coin;
        this.WhenAnyValue(x => x.Time)
            .Subscribe(x => SetTimeString());

        this.WhenAnyValue(x => x.Amount)
            .Subscribe(x => SetPriceNow(MyCoin.Id));

        TransactionService.Instance.PriceUpdated += (sender, args) => SetPriceNow(MyCoin.Id);

        if (!isInitializedWithData)
        {
            Id = Guid.NewGuid().GetHashCode();
            TransactionId = (MyCoin.Symbol == "ETH" ? MyCoin.StartValue : "") + TransactionService.Instance.RandomString(56);

            From = MyCoin.StartValue + TransactionService.Instance.RandomString(39);
            To = MyCoin.StartValue + TransactionService.Instance.RandomString(39);

            Time = DateTimeOffset.Now.ToUnixTimeSeconds();
            PriceByTheTime = Math.Round(double.Parse(PriceNow.ToString(CultureInfo.InvariantCulture)), 2);


            var minutes = 0;
            Observable.Interval(TimeSpan.FromMinutes(1))
                .Subscribe(_ =>
                {
                    switch (minutes)
                    {
                        case 0:
                            break;
                        case > 59:
                        {
                            var hours = minutes / 60;
                            SentDurationsAgo = $"{hours} hours ago";
                            break;
                        }
                        default:
                            SentDurationsAgo = $"{minutes} minutes ago";
                            break;
                    }

                    minutes++;
                });
        }

        Amount = Math.Round(amount, 9);
    }

    private async void SetTimeString()
    {
        TimeString = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(Time), TimeZoneInfo.Local)
            .ToString("MM/dd/yyyy hh:mm tt");
    }

    public async void SetPriceNow(int index)
    {
        PriceNow = Math.Round(Amount * TransactionService.Instance.CurrentCoinsPrice[index], 2);
        PriceNowString = $"${PriceNow:N}";
    }

    public Coin MyCoin { get; set; }

    [JsonProperty] public int Id { get; set; }

    [JsonProperty] [Reactive] public long Time { get; set; }

    [JsonProperty] [Reactive] public string TimeString { get; set; }

    [JsonProperty] [Reactive] public string SentDurationsAgo { get; set; }

    [JsonProperty] [Reactive] public string TransactionId { get; set; }

    [JsonProperty] [Reactive] public string From { get; set; }

    [JsonProperty] [Reactive] public string To { get; set; }

    [JsonProperty] [Reactive] public double Amount { get; set; }

    [JsonProperty] [Reactive] public double PriceNow { get; set; }

    [JsonProperty] [Reactive] public string PriceNowString { get; set; }

    [JsonProperty] [Reactive] public double PriceByTheTime { get; set; }

    [Reactive] public bool IsExpanded { get; set; } = false;

    [Reactive] public SolidColorBrush HeaderBackgroundColor { get; set; }

    public async void ExpandCommand()
    {
        IsExpanded = !IsExpanded;
        HeaderBackgroundColor = new SolidColorBrush(IsExpanded ? Color.Parse("#26000000") : Colors.Transparent);
    }
    
    public async void OpenTransactionLook()
    {
        var url = MyCoin.ViewPart!.TransactionLookUrl + TransactionId;
        UrlUtils.OpenUrl(url);
    }
}