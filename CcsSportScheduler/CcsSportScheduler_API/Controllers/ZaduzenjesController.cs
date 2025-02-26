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

        // POST: api/Zaduzenjes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Post(ZaduzenjeRequest zaduzenjeRequest)
        {
            try
            {
                decimal placeno = zaduzenjeRequest.Placeno;
                if(zaduzenjeRequest.Type == (int)FinancialCardTypeEnumeration.Clanarina)
                {
                    if(zaduzenjeRequest.NewTypeUser == null)
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
                    else if(zaduzenjeRequest.NewTypeUser.Value == (int)UserEnumeration.Fiksni ||
                        zaduzenjeRequest.NewTypeUser.Value == (int)UserEnumeration.Trenerski)
                    {
                        TerminRequest terminRequest = new TerminRequest()
                        {
                            TerenId = zaduzenjeRequest.Teren.Value,
                            UserId = zaduzenjeRequest.UserId,
                            Zaduzi = 0
                        };

                        int year = DateTime.Now.Year;

                        DayOfWeek dayOfWeek = ConvertToDayOfWeek(zaduzenjeRequest.Dan.Value);

                        List<DateTime> dates = GetDatesForDayOfWeek(year, dayOfWeek);

                        foreach (var date in dates)
                        {
                            terminRequest.StartDateTime = new DateTime(date.Year, date.Month, date.Day, zaduzenjeRequest.Sat.Value, 0, 0);

                            // Dohvati cene termina sa API-ja
                            var client = _httpClientFactory.CreateClient("MyHttpClient");
                            var responseCenaTermina = await client.PostAsJsonAsync($"/api/Termins/zakazi", terminRequest);

                            if(!responseCenaTermina.IsSuccessStatusCode)
                            {
                                return BadRequest("Error while creating termin.");
                            }
                        }
                    }

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                else if(zaduzenjeRequest.Type == (int)FinancialCardTypeEnumeration.OtpisPozajmice)
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
        private List<DateTime> GetDatesForDayOfWeek(int year, DayOfWeek dayOfWeek)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime startDate = new DateTime(year, 1, 1);

            // Pronađi prvi željeni dan u nedelji
            while (startDate.DayOfWeek != dayOfWeek)
            {
                startDate = startDate.AddDays(1);
            }

            // Dodaj svaki željeni dan u nedelji do kraja godine
            while (startDate.Year == year)
            {
                dates.Add(startDate);
                startDate = startDate.AddDays(7); // Dodaj 7 dana da dobiješ sledeći željeni dan u nedelji
            }

            return dates;
        }
        private bool ZaduzenjeExists(string id)
        {
          return (_context.Zaduzenja?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
