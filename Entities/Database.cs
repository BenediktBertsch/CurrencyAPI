using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ExchangeRatesAPI.Entities
{

    public class ExchangeRates
    {
        public int id { get; set; }
        public DateTime date { get; set; }
        public virtual List<Exchange> currencies { get; set; }
        public ExchangeRates() { }
        public ExchangeRates(DateTime date, List<Exchange> currencies)
        {
            this.date = date;
            this.currencies = currencies;
        }
    }

    public class Exchange
    {
        public int id { get; set; }
        public string currency { get; set; }
        public decimal rate { get; set; }
        public Exchange() { }
        public Exchange(string currency, decimal rate)
        {
            this.currency = currency;
            this.rate = rate;
        }
    }

    public class DatabaseContext : DbContext
    {
        public DbSet<ExchangeRates> Rates { get; set; }
        public DatabaseContext(DbContextOptions opt) : base(opt)
        {

        }
    }
}
