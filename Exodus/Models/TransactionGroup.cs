using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Exodus.Models;

public class TransactionGroup : ReactiveObject
{
    public TransactionGroup(DateTime date)
    {
        Date = date;
        
        if (date == DateTime.Today)
        {
            DateTitle = "Today";
        }
        else if (date == DateTime.Today.AddDays(-1))
        {
            DateTitle = "Yesterday";
        }
        else
        {
            DateTitle = date.ToString("MMM dd, yyyy");
        }
        
        Transactions!.CollectionChanged += (sender, args) =>
        {
            IsThereTransaction = Transactions.Count > 0;
        };
    }
    
    [Reactive] public DateTime Date { get; set; }
    
    [Reactive] public string? DateTitle { get; set; }
    
    [Reactive] public bool IsThereTransaction { get; set; }

    [Reactive] public ObservableCollection<Transaction>? Transactions { get; set; } = new();
}