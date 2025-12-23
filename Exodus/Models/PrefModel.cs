using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Exodus.Models;

public class PrefModel
{
    /// <summary>
    /// Coins data
    /// </summary>
    [JsonProperty]
    public ObservableCollection<Coin>? Coins { get; set; }
}