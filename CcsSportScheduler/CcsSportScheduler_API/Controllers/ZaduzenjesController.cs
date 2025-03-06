using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using CcsSportScheduler_API.Models.Requests.Uplata;
using CcsSportScheduler_API.Models.Requests.Zaduzenje;
using CcsSportScheduler_API.Enumeration;
using CcsSportScheduler_API.Models.Requests.Teren;
using CcsSportScheduler_API.Models.Response;
using CcsSportScheduler_API.Models.Requests.Racun;
using CcsSportScheduler_API.Models.Response.FinancialCard;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZaduzenjesController : ControllerBase
    {
        private readonly SportSchedulerContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ZaduzenjesController(SportSchedulerContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var zaduzenje = await _context.Zaduzenja.FindAsync(id);
            if (zaduzenje == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "ZaduzenjesController",
                    Message = "Ne postoji zaduzenje u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            return Ok(zaduzenje);
        }
        // POST: api/Zaduzenjes
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Post(ZaduzenjeRequest zaduzenjeRequest)
        {
            try
            {
                if (zaduzenjeRequest.Placeno == 0)
                {
                    decimal pretplata = await GetPretplata(zaduzenjeRequest.UserId);

                    if (pretplata > 0)
                    {
                        if (pretplata >= zaduzenjeRequest.TotalAmount)
                        {
                            zaduzenjeRequest.Placeno = zaduzenjeRequest.TotalAmount;
                        }
                        else
                        {
                            zaduzenjeRequest.Placeno = pretplata;
                        }
                    }
                }

                decimal placeno = zaduzenjeRequest.Placeno;
                if (zaduzenjeRequest.Type == (int)FinancialCardTypeEnumeration.Clanarina)
                {
                    if (zaduzenjeRequest.NewTypeUser == null)
                    {
                        return BadRequest("New type user is required.");
                    }

                    var user = await _context.Users.FindAsync(zaduzenjeRequest.UserId);

                    if (user == null)
                    {
                        return NotFound("User not found.");
                    }

                    user.Type = zaduzenjeRequest.NewTypeUser.Value;

                    if (zaduzenjeRequest.NewTypeUser.Value == (int)UserEnumeration.Plivajuci)
                    {
                        user.FreeTermin = 4;
                    }

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    if (zaduzenjeRequest.NewTypeUser.Value == (int)UserEnumeration.Fiksni ||
                        zaduzenjeRequest.NewTypeUser.Value == (int)UserEnumeration.Trenerski)
                    {
                        int year = DateTime.Now.Year;

                        DayOfWeek dayOfWeek = ConvertToDayOfWeek(zaduzenjeRequest.Dan.Value);

                        TerminFiksniRequest terminRequest = new TerminFiksniRequest()
                        {
                            TerenId = zaduzenjeRequest.Teren.Value,
                            UserId = zaduzenjeRequest.UserId,
                            Zaduzi = 0,
                            Dates = GetDatesForDayOfWeek(year, dayOfWeek, zaduzenjeRequest.Sat.Value)
                        };

                        // Dohvati cene termina sa API-ja
                        var client = _httpClientFactory.CreateClient("MyHttpClient");
                        var responseCenaTermina = await client.PostAsJsonAsync($"/api/Termins/zakazi/fiksni", terminRequest);

                        if (!responseCenaTermina.IsSuccessStatusCode)
                        {
                            return BadRequest("Error while creating termin.");
                        }
                    }
                }
                else if (zaduzenjeRequest.Type == (int)FinancialCardTypeEnumeration.OtpisPozajmice)
                {
                    placeno = zaduzenjeRequest.TotalAmount;
                }

                Zaduzenje zaduzenje = new Zaduzenje()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = zaduzenjeRequest.UserId,
                    Date = TimeZoneInfo.ConvertTime(zaduzenjeRequest.Date,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")),
                    Opis = zaduzenjeRequest.Opis,
                    TotalAmount = zaduzenjeRequest.TotalAmount,
                    Type = zaduzenjeRequest.Type,
                    Otpis = 0,
                    Placeno = placeno,
                };

                _context.Zaduzenja.Add(zaduzenje);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private DayOfWeek ConvertToDayOfWeek(int danNedelje)
        {
            return danNedelje switch
            {
                0 => DayOfWeek.Monday,
                1 => DayOfWeek.Tuesday,
                2 => DayOfWeek.Wednesday,
                3 => DayOfWeek.Thursday,
                4 => DayOfWeek.Friday,
                5 => DayOfWeek.Saturday,
                6 => DayOfWeek.Sunday,
                _ => throw new ArgumentOutOfRangeException(nameof(danNedelje), danNedelje, null)
            };
        }
        private List<DateTime> GetDatesForDayOfWeek(int year, 
            DayOfWeek dayOfWeek,
            int sat)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime startDate = new DateTime(year, 3, 1, sat, 0, 0);
            DateTime endDate = new DateTime(year, 10, 31, 23, 0, 0);

            // Pronađi prvi željeni dan u nedelji
            while (startDate.DayOfWeek != dayOfWeek)
            {
                startDate = startDate.AddDays(1);
            }

            // Dodaj svaki željeni dan u nedelji do endtDate
            while (startDate.Date < endDate.Date)
            {
                dates.Add(startDate);
                startDate = startDate.AddDays(7); // Dodaj 7 dana da dobiješ sledeći željeni dan u nedelji
            }

            return dates;
        }
        private async Task<decimal> GetPretplata(int userId)
        {
            try
            {
                decimal pretplata = 0;

                DateTime from = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime to = new DateTime(DateTime.Now.Year, 12, 31);

                var clanarice = await GetAllClanarice(userId, from, to);
                var termini = await GetAllTermins(userId, from, to);
                var kafic = await GetAllKafic(userId, from, to);
                var prodavnica = await GetAllProdavnica(userId, from, to);
                var kotizacije = await GetAllKotizacija(userId, from, to);
                var otpisPozajmice = await GetAllOtpisPozajmice(userId, from, to);
                var uplate = await GetAllUplate(userId, from, to);
                var pokloni = await GetAllPoklon(userId, from, to);
                var pozajmica = await GetAllPozajmica(userId, from, to);
                var otkazTermina = await GetAllOtkazTermina(userId, from, to);

                var items = new List<FinancialCardItemResponse>();
                items.AddRange(clanarice);
                items.AddRange(termini);
                items.AddRange(kafic);
                items.AddRange(prodavnica);
                items.AddRange(kotizacije);
                items.AddRange(otpisPozajmice);
                items.AddRange(uplate);
                items.AddRange(pokloni);
                items.AddRange(pozajmica);
                items.AddRange(otkazTermina);

                decimal totalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate ||
                        i.Type == FinancialCardTypeEnumeration.Poklon ||
                        i.Type == FinancialCardTypeEnumeration.Pozajmica ||
                        i.Type == FinancialCardTypeEnumeration.OtkazTermina).Sum(u => u.Razduzenje);

                decimal totalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kafic ||
                i.Type == FinancialCardTypeEnumeration.Termini ||
                i.Type == FinancialCardTypeEnumeration.Kotizacije ||
                i.Type == FinancialCardTypeEnumeration.Prodavnica ||
                i.Type == FinancialCardTypeEnumeration.OtpisPozajmice ||
                i.Type == FinancialCardTypeEnumeration.Clanarina).Sum(t => t.Zaduzenje);

                return totalRazduzenje - totalZaduzenje;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        private async Task<List<FinancialCardItemResponse>> GetAllClanarice(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var clanarine = _context.Zaduzenja.Where(z => z.UserId == id &&
                z.Date >= from.Date && z.Date <= to &&
                z.Type == (int)FinancialCardTypeEnumeration.Clanarina);

                if (clanarine.Any())
                {
                    foreach (var c in clanarine)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = c.Id,
                            Type = FinancialCardTypeEnumeration.Clanarina,
                            Date = c.Date,
                            Razduzenje = c.Placeno,
                            Zaduzenje = c.TotalAmount,
                            Otpis = c.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllOtpisPozajmice(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var clanarine = _context.Zaduzenja.Where(z => z.UserId == id &&
                z.Date >= from.Date && z.Date <= to &&
                z.Type == (int)FinancialCardTypeEnumeration.OtpisPozajmice);

                if (clanarine.Any())
                {
                    foreach (var c in clanarine)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = c.Id,
                            Type = FinancialCardTypeEnumeration.OtpisPozajmice,
                            Date = c.Date,
                            Razduzenje = c.Placeno,
                            Zaduzenje = c.TotalAmount,
                            Otpis = c.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllTermins(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var termins = _context.Termins.Where(t => t.UserId == id && t.StartDateTime >= from && t.StartDateTime <= to);

                if (termins.Any())
                {
                    foreach (var t in termins)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = t.Id,
                            Type = FinancialCardTypeEnumeration.Termini,
                            Date = t.StartDateTime,
                            Razduzenje = t.Placeno,
                            Zaduzenje = t.Price,
                            Otpis = t.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllKafic(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var kafic = _context.Racuns.Where(r => r.UserId == id &&
                r.Date >= from && r.Date <= to &&
                r.Type == (int)FinancialCardTypeEnumeration.Kafic);

                if (kafic.Any())
                {
                    foreach (var r in kafic)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = r.Id,
                            Type = FinancialCardTypeEnumeration.Kafic,
                            Date = r.Date,
                            Razduzenje = r.Placeno,
                            Zaduzenje = r.TotalAmount,
                            Otpis = r.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllProdavnica(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var kafic = _context.Racuns.Where(r => r.UserId == id &&
                r.Date >= from && r.Date <= to &&
                r.Type == (int)FinancialCardTypeEnumeration.Prodavnica);

                if (kafic.Any())
                {
                    foreach (var r in kafic)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = r.Id,
                            Type = FinancialCardTypeEnumeration.Prodavnica,
                            Date = r.Date,
                            Razduzenje = r.Placeno,
                            Zaduzenje = r.TotalAmount,
                            Otpis = r.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllKotizacija(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var kafic = _context.Zaduzenja.Where(r => r.UserId == id &&
                r.Date >= from && r.Date <= to &&
                r.Type == (int)FinancialCardTypeEnumeration.Kotizacije);

                if (kafic.Any())
                {
                    foreach (var r in kafic)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = r.Id,
                            Type = FinancialCardTypeEnumeration.Kotizacije,
                            Date = r.Date,
                            Razduzenje = r.Placeno,
                            Zaduzenje = r.TotalAmount,
                            Otpis = r.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllUplate(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var uplate = _context.Uplata.Where(u => u.UserId == id &&
                u.Date >= from && u.Date <= to &&
                u.TypeUplata == (int)UplataEnumeration.Standard);

                if (uplate.Any())
                {
                    foreach (var u in uplate)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = u.Id,
                            Type = FinancialCardTypeEnumeration.Uplate,
                            Date = u.Date,
                            Razduzenje = u.TotalAmount,
                            Zaduzenje = 0,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllPoklon(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var poklon = _context.Uplata.Where(p => p.UserId == id &&
                p.Date >= from && p.Date <= to &&
                p.TypeUplata == (int)UplataEnumeration.Poklon);

                if (poklon.Any())
                {
                    foreach (var p in poklon)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = p.Id,
                            Type = FinancialCardTypeEnumeration.Poklon,
                            Date = p.Date,
                            Razduzenje = p.TotalAmount,
                            Zaduzenje = 0,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllPozajmica(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var poklon = _context.Uplata.Where(p => p.UserId == id && p.Date >= from &&
                p.Date <= to &&
                p.TypeUplata == (int)UplataEnumeration.Pozajmica);

                if (poklon.Any())
                {
                    foreach (var p in poklon)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = p.Id,
                            Type = FinancialCardTypeEnumeration.Pozajmica,
                            Date = p.Date,
                            Razduzenje = p.TotalAmount,
                            Zaduzenje = 0,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private async Task<List<FinancialCardItemResponse>> GetAllOtkazTermina(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var otkazTermina = _context.Uplata.Where(p => p.UserId == id && p.Date >= from &&
                p.Date <= to &&
                p.TypeUplata == (int)UplataEnumeration.OtkazTermina);

                if (otkazTermina.Any())
                {
                    foreach (var p in otkazTermina)
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = p.Id,
                            Type = FinancialCardTypeEnumeration.OtkazTermina,
                            Date = p.Date,
                            Razduzenje = p.TotalAmount,
                            Zaduzenje = 0,
                        };

                        items.Add(financialCardItemResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
        private bool ZaduzenjeExists(string id)
        {
          return (_context.Zaduzenja?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
