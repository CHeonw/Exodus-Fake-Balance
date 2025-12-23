using Exodus.Views;
using ReactiveUI.Fody.Helpers;

namespace Exodus.ViewModels;

public class UpdateAddressWindowViewModel
{
    public UpdateAddressWindowViewModel(UpdateAddressWindow window, string address)
    {
        _window = window;
        DraftAddress = address;
    }
    
    private UpdateAddressWindow _window;
    
    public bool SuccessfullyUpdated { get; set; } = false;

    [Reactive] public string DraftAddress { get; set; }
    
    public string NewAddress { get; set; }
    
    public void Save()
    {
        NewAddress = DraftAddress;
        SuccessfullyUpdated = true;
        Close();
    }

    public void Close()
    {
        _window.Close();
    }
}