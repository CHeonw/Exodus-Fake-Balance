using System;
using Exodus.Models;

namespace Exodus.Services;

public class CoinManagementService
{
    public static CoinManagementService Instance { get; } = new();

    public event EventHandler CoinSelectedEvent;

    public void CoinSelected(Coin coin)
    {
        CoinSelectedEvent?.Invoke(coin, EventArgs.Empty);
    }
}