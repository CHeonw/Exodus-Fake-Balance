using System;
using Exodus.Models;

namespace Exodus.Services;

public class GeneralServices
{
    public static GeneralServices Instance { get; } = new();
    
    public event EventHandler CopyTextToClipBoardEvent;

    public void CopyTextToClipBoard(string text)
    {
        CopyTextToClipBoardEvent?.Invoke(text, EventArgs.Empty);
    }
}