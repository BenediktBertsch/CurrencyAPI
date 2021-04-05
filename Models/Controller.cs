using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeRatesAPI.Models
{
    public class ExchangeModel
    {
        long timestamp { get; set; }
        string sourceBase { get; set; }
        List<KeyValuePair<string, decimal>> quotes { get; set; }
        public ExchangeModel(long timestamp, string sourceBase, List<KeyValuePair<string, decimal>> quotes)
        {
            this.timestamp = timestamp;
            this.sourceBase = sourceBase;
            this.quotes = quotes;
        }
    }
}
