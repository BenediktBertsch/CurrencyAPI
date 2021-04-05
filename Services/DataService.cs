using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRatesAPI.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Serialization;
using ExchangeRatesAPI.Models;
using System.IO;

namespace ExchangeRatesAPI.Services
{
    public class DataService : IHostedService
    {
        // Urls
        const string daily = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
        const string threeMonths = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml";
        const string history = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

        private readonly IServiceScopeFactory _scopeFactory;
        private HttpClient _client;
        private DatabaseContext _dbContext;
        private Task _checkForUpdate;
        public DataService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            var scope = _scopeFactory.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            _dbContext.Database.EnsureCreated();
            _client = new HttpClient();
            DataServiceAsync().Wait();
        }

        public async Task DataServiceAsync()
        {
            try
            {
                // Check if data older than 90 days exist
                var check = _dbContext.Rates.FirstOrDefault((v) => v.date <= DateTime.Now.AddDays(-90));
                if (check != null)
                {
                    // Check if data older than 2 days exist
                    check = _dbContext.Rates.FirstOrDefault((v) => v.date <= DateTime.Now.AddDays(-2));
                    if(check != null)
                    {
                        // Check if data for today exist
                        check = _dbContext.Rates.FirstOrDefault((v) => v.date <= DateTime.Now);
                        if(check == null)
                        {
                            Console.WriteLine("Downloading latest data...");
                            await GetDaily();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Downloading last 90 days...");
                        await GetThreeMonths();
                    }
                } 
                else
                {
                    Console.WriteLine("Downloading history...");
                    await GetHistory();
                }

                // Start Background task which checks every 24 hours for updates
                _checkForUpdate = Task.Run(async() => await CheckForUpdateAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task CheckForUpdateAsync()
        {
            while (true)
            {
                var date = DateTime.Now;
                if(date.Hour == 16 && date.Minute == 0)
                {
                    Console.WriteLine("Checking for new Data...");
                    await GetDaily();
                }
                Thread.Sleep(30000); // Check every minute
            }
        }

        public async Task GetDaily()
        {
            var parse = await (await _client.GetAsync(daily)).Content.ReadAsStringAsync();
            ParseString(parse);
        }

        public async Task GetThreeMonths()
        {
            var parse = await (await _client.GetAsync(threeMonths)).Content.ReadAsStringAsync();
            ParseString(parse);
        }

        public async Task GetHistory()
        {
            var parse = await (await _client.GetAsync(history)).Content.ReadAsStringAsync();
            ParseString(parse);
        }

        private void ParseString(string toParse)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Envelope));
            Envelope result;
            using (TextReader reader = new StringReader(toParse))
            {
                result = (Envelope)ser.Deserialize(reader);
            }
            result.Cube.ToList().ForEach((v) =>
            {
                // Check if data is new for DB
                var found = _dbContext.Rates.Where((r) => r.date == v.time).FirstOrDefault();
                if (found == null)
                {
                    var temp = new List<Exchange>();
                    v.Cube.ToList().ForEach((s) =>
                    {
                        temp.Add(new Exchange(s.currency, s.rate));
                    });
                    _dbContext.Rates.Add(new ExchangeRates(v.time, temp));
                }
            });
            _dbContext.SaveChanges();
            Console.WriteLine("Check and download finished.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
