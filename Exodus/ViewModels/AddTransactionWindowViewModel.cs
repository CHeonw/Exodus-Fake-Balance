using Exodus.Models;
using Exodus.Services;
using Exodus.Views;
using ReactiveUI.Fody.Helpers;

namespace Exodus.ViewModels;

public class AddTransactionWindowViewModel : ViewModelBase
{
    public AddTransactionWindowViewModel()
    {
        
    }
    
    private AddTransactionWindow _window;
    
    public AddTransactionWindowViewModel(AddTransactionWindow window, Coin myCoin, int myCoinIndex)
    {
        _window = window;
        
        var amount = 857684 / TransactionService.Instance.CurrentCoinsPrice[myCoin.Id];
        MyTransaction = new Transaction();
        MyTransaction.SetConst(amount, myCoin);
    }
    
    [Reactive] public Transaction MyTransaction { get; set; }

    public bool SuccessfullyAdd { get; set; } = false;

    public void Save()
    {
        SuccessfullyAdd = true;
        Close();
    }

    public void Close()
    {
        _window.Close();
    }
}