using ExchangeRatesAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExchangeRatesAPI.Models;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ExchangeRatesAPI.Controllers
{
#nullable enable
#pragma warning disable 8604
    public class ExchangeController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public ExchangeController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("latest")]
        public IActionResult Latest(string? Base = "", string? symbols = "")
        {
            try
            {
                var dbLatest = _dbContext.Rates.Include(r => r.currencies).OrderByDescending(d => d.date).FirstOrDefault();
                return BaseMethod(dbLatest, Base, symbols);
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    error = e.Message
                });
            }
        }

        [HttpGet]
        [Route("history")]
        public IActionResult History(string? start_at, string? end_at, string? Base, string? symbols)
        {
            try
            {
                DateTime startAt, endAt;
                if (start_at != null && end_at != null)
                {
                    startAt = DateTime.ParseExact(start_at, new string[] { "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture);
                    endAt = DateTime.ParseExact(end_at, new string[] { "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture);

                    if (endAt < startAt)
                        return BadRequest(new { error = "Date end_at can not be earlier in time than start_at." });

                    var results = _dbContext.Rates.Include(r => r.currencies).Where(r => r.date >= startAt && r.date <= endAt).OrderBy(d => d.date).ToList();

                    return BaseMethodHistory(results, Base, symbols);
                }
                else if (start_at != null && end_at == null)
                {
                    startAt = DateTime.ParseExact(start_at, new string[] { "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture);

                    var results = _dbContext.Rates.Include(r => r.currencies).Where(r => r.date >= startAt).OrderBy(d => d.date).ToList();

                    return BaseMethodHistory(results, Base, symbols);
                }
                else if (start_at == null && end_at != null)
                {
                    endAt = DateTime.ParseExact(end_at, new string[] { "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture);

                    var results = _dbContext.Rates.Include(r => r.currencies).Where(r => r.date <= endAt).OrderBy(d => d.date).ToList();

                    return BaseMethodHistory(results, Base, symbols);
                }
                else
                {
                    return BadRequest(new { error = "start_at or end_at parameter needed." });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpGet]
        [Route("{date}")]
        public IActionResult Date(string date = "yyyy-mm-dd", string? Base = "", string? symbols = "")
        {
            // Validate date yyyy-mm-dd
            DateTime inputDate;
            if (date != "yyyy-mm-dd" && DateTime.TryParseExact(date, new string[] { "yyyy-MM-dd", "dd-MM-yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out inputDate))
            {
                // Check if date is earlier than 1999-01-04
                if (inputDate < new DateTime(1999, 01, 04))
                    return BadRequest(new { error = "Date can't be earlier than 1999-01-04." });
                var result = _dbContext.Rates.Include(r => r.currencies).Where(r => r.date == inputDate).FirstOrDefault();
                return BaseMethod(result, Base, symbols);
            }
            else
            {
                return BadRequest(new { error = "Needs date parameter in yyyy-mm-dd/dd-mm-yyyy format." });
            }
        }

        public IActionResult BaseMethodHistory(List<ExchangeRates> dbList, string? Base = "", string? symbols = "")
        {
            var exo = new Dictionary<string, dynamic>();
            if (Base != "" && Base != null)
            {
                decimal baseRate = 0;
                dbList.ForEach((r) =>
                {
                    r.currencies.ForEach((c) =>
                    {
                        if (c.currency == Base)
                        {
                            baseRate = c.rate;
                        }
                    });
                    if (!exo.ContainsKey(r.date.ToString("yyyy-MM-dd")))
                        exo.Add(r.date.ToString("yyyy-MM-dd"), Check(r, Base, new ExpandoObject(), baseRate, symbols));
                });

                return Ok(new { timestamp = Math.Floor((DateTime.Now - Epoch).TotalSeconds), source = Base, rates = exo });
            }
            else
            {
                Base = "EUR";
                for (int i = 0; i < dbList.Count; i++)
                {
                    if (!exo.ContainsKey(dbList[i].date.ToString("yyyy-MM-dd")))
                        exo.Add(dbList[i].date.ToString("yyyy-MM-dd"), Check(dbList[i], Base, new ExpandoObject(), null, symbols));
                }
                return Ok(new { timestamp = Math.Floor((DateTime.Now - Epoch).TotalSeconds), source = Base, rates = exo });
            }
        }

        public IActionResult BaseMethod(ExchangeRates dbValue, string? Base, string? symbols)
        {
            dynamic exo = new ExpandoObject();
            if (Base != "")
            {
                var found = false;
                decimal baseRate = 0;
                dbValue.currencies.ForEach((cur) =>
                {
                    if (cur.currency == Base.ToUpper())
                    {
                        found = true;
                        baseRate = cur.rate;
                    }
                });

                if (!found && Base.ToUpper() != "EUR")
                    return BadRequest();

                exo = Check(dbValue, Base, exo, baseRate, symbols);
                return Ok(new { timestamp = (dbValue.date - Epoch).TotalSeconds, source = Base, quotes = exo });
            }
            else
            {
                Base = "EUR";
                exo = Check(dbValue, Base, exo, null, symbols);
                return Ok(new { timestamp = (dbValue.date - Epoch).TotalSeconds, source = Base, quotes = exo });
            }
        }

        public dynamic Check(ExchangeRates dbValue, string Base, dynamic exo, decimal? baseRate, string? symbols)
        {
            var symbolList = new List<string>();
            var symbolCheck = false;
            if (symbols != null)
            {
                try
                {
                    symbolList = symbols.Split(',').ToList();
                    symbolCheck = true;
                }
                catch (Exception)
                {
                    symbolCheck = false;
                }
            }

            dbValue.currencies.ForEach((cur) =>
            {
                if (Base.ToUpper() != cur.currency)
                {
                    if (symbolCheck && !symbolList.Contains(cur.currency) && baseRate != null)
                    {
                        if (baseRate != 0)
                            ((IDictionary<String, Object>)exo).Add(cur.currency, cur.rate / baseRate);
                        else
                            ((IDictionary<String, Object>)exo).Add(cur.currency, cur.rate);
                    }
                    else
                    {
                        if (baseRate != null && baseRate != 0)
                            ((IDictionary<String, Object>)exo).Add(cur.currency, cur.rate / baseRate);
                        else
                            ((IDictionary<String, Object>)exo).Add(cur.currency, cur.rate);
                    }
                }
                else
                {
                    if (baseRate != null || baseRate != 0)
                        ((IDictionary<String, Object>)exo).Add(cur.currency, cur.rate / baseRate);
                    else
                        ((IDictionary<String, Object>)exo).Add(cur.currency, cur.rate);
                }
            });

            if (Base.ToUpper() != "EUR")
            {
                if (symbolCheck && !symbolList.Contains("EUR"))
                    ((IDictionary<String, Object>)exo).Add("EUR", baseRate);
                else if (!symbolCheck)
                    ((IDictionary<String, Object>)exo).Add("EUR", baseRate);
            }

            return exo;
        }
    }
}
