using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exodus.Models;

public class Price
{
    public Dictionary<string, CoinData> data { get; set; }
}

public class CoinData
{
    public Quote quote { get; set; }
}

public class Quote
{
    public USD USD { get; set; }
}

public class USD
{
    public double price { get; set; }
}