using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Exodus.Models;
using Newtonsoft.Json;

namespace Exodus.Services;

public class TransactionService
{
    public static TransactionService Instance { get; } = new TransactionService();

    public TransactionService()
    {
        CurrentCoinsPriceApi();
        Observable.Interval(TimeSpan.FromMinutes(5))
            .Subscribe(_ => CurrentCoinsPriceApi());
    }

    public event EventHandler? PriceUpdated;

    private const string ApiKey = "7b71a7d2-7174-468b-8432-93b0cb4df622";
    private const string Url = "https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol=";
    
    
    public List<double> CurrentCoinsPrice { get; private set; } = new() { 60000, 3400 };
    
    public List<string> CoinsSymbol { get; } = new() { "BTC", "ETH" };

    private async void CurrentCoinsPriceApi()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", ApiKey);
        client.DefaultRequestHeaders.Add("Accepts", "application/json");
        
        client.Timeout = TimeSpan.FromSeconds(5);

        var isUpdated = false;
        var i = 0;
        while (i < 2)
        {
            try
            {
                var response = await client.GetAsync(Url + CoinsSymbol[i]);
                if (!response.IsSuccessStatusCode) throw new Exception();
            
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var price = JsonConvert.DeserializeObject<Price>(jsonResponse);
                if (price == null) return;

                CurrentCoinsPrice[i] = price.data[CoinsSymbol[i]].quote.USD.price;
                CurrentCoinsPrice[i] = Math.Round(CurrentCoinsPrice[i], 2);
                isUpdated = true;
            }
            catch (Exception)
            {
                // ignore
            }
            i++;
        }
        
        if (isUpdated) PriceUpdated?.Invoke(this, EventArgs.Empty);
    }

    public string RandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public event EventHandler AddTransactionEvent;

    public void AddTransaction()
    {
        AddTransactionEvent.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler TransactionAddedEvent;

    public void TransactionAdded()
    {
        TransactionAddedEvent.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler StopPendingEvent;

    public void StopPending()
    {
        StopPendingEvent.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler DeleteLastTransactionEvent;

    public void DeleteLastTransaction()
    {
        DeleteLastTransactionEvent.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler ResetEvent;

    public void Reset()
    {
        ResetEvent.Invoke(this, EventArgs.Empty);
    }
    
    public event EventHandler ChangeAddressEvent;

    public void ChangeAddress()
    {
        ChangeAddressEvent.Invoke(this, EventArgs.Empty);
    }
}