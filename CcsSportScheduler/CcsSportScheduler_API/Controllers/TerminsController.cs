using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using CcsSportScheduler_API.Enumeration;
using CcsSportScheduler_API.Models.Response;
using CcsSportScheduler_API.Models.Requests.Racun;
using CcsSportScheduler_API.Models.Requests.Teren;
using System.Net.Http;

namespace CcsSportScheduler_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TerminsController : ControllerBase
    {
        private readonly SportSchedulerContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public TerminsController(SportSchedulerContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/Termins/zakazaniTermini/5
        [Route("zakazaniTermini/{idTeren}/{idUser}")]
        [HttpGet]
        public async Task<IActionResult> ZakazaniTermini(int idTeren, int idUser, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var terenDB = await _context.Terens.FindAsync(idTeren);

                if (terenDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Traženi teren ne postoji u bazi podataka.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "ZakazaniTermini"
                    });
                }
                var userDB = await _context.Users.FindAsync(idUser);

                if (userDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji korisnik u bazi podataka.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "ZakazaniTermini"
                    });
                }

                // Dohvati cene termina sa API-ja
                var client = _httpClientFactory.CreateClient("MyHttpClient");
                var responseCenaTermina = await client.GetAsync($"/api/klubs/naplataTermina/{idUser}");

                if (!responseCenaTermina.IsSuccessStatusCode)
                {
                    return StatusCode((int)responseCenaTermina.StatusCode, new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Greška prilikom dohvatanja cena termina.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "ZakazaniTermini"
                    });
                }

                var cenaTermina = await responseCenaTermina.Content.ReadFromJsonAsync<NaplataTermina>();


                if (!startDate.HasValue)
                {
                    startDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
                }
                DateTime start = TimeZoneInfo.ConvertTime(new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 7, 0, 0),
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));


                DateTime end = start.AddDays(6).AddHours(16);

                // Filtriranje termina u zadanom periodu
                var termini = _context.Termins.Where(t => t.TerenId == idTeren &&
                t.StartDateTime >= start && t.StartDateTime <= end).Include(t => t.User).ToList();

                // Generisanje svih mogućih termina u zadanom periodu
                var allTermini = new List<Termin>();
                for (var date = start; date <= end; date = date.AddHours(1))
                {
                    if (date.Hour < 7 || date.Hour > 22)
                    {
                        continue; // Preskoči termine van intervala 7-22h
                    }

                    // Proveravamo da li je termin već zakazan
                    var existingTermin = termini.FirstOrDefault(t => t.StartDateTime == date);
                    if (existingTermin == null)
                    {
                        // Ako termin nije zakazan, dodajemo nezakazani termin sa odgovarajućom cenom
                        var dayOfWeek = date.DayOfWeek;
                        var isWeekend = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday);

                        allTermini.Add(new Termin
                        {
                            StartDateTime = date,
                            EndDateTime = date.AddHours(1),
                            TerenId = idTeren,
                            User = null, // Oznaka da je termin nezakazan
                            Price = cenaTermina == null ? 600 : cenaTermina.Price // Postavljanje cene za nezakazane termine
                        });
                    }
                    else
                    {
                        // Ako je termin zakazan, dodajemo ga
                        allTermini.Add(existingTermin);
                    }
                }

                // Uklonite duplirane termine
                termini = allTermini.Distinct().ToList();

                // Sada `termini` sadrži sve termine, zakazane i nezakazane

                return Ok(termini);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Route("zakazaniTermini/{idTeren}")]
        //[HttpGet]
        //public async Task<IActionResult> ZakazaniTermini(int idTeren, DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    var terenDB = await _context.Terens.FindAsync(idTeren);

        //    if (terenDB == null)
        //    {
        //        return NotFound(new ErrorResponse
        //        {
        //            Controller = "TerminsController",
        //            Message = "Traženi teren ne postoji u bazi podataka.",
        //            Code = ErrorEnumeration.NotFound,
        //            Action = "ZakazaniTermini"
        //        });
        //    }

        //    //var termini = _context.Termins.Where(t => t.TerenId == idTeren).Include(t => t.User);

        //    //if (startDate.HasValue && endDate.HasValue)
        //    //{
        //    //    DateTime start = new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0);
        //    //    DateTime end = new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59);

        //    //    termini = termini.Where(t => t.StartDateTime >= start &&
        //    //    t.StartDateTime <= end).Include(t => t.User);
        //    //}

        //    var termini = _context.Termins.Where(t => t.TerenId == idTeren).Include(t => t.User).ToList();

        //    if (startDate.HasValue && endDate.HasValue)
        //    {
        //        DateTime start = new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 0, 0, 0);
        //        DateTime end = new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, 23, 59, 59);

        //        termini = termini.Where(t => t.StartDateTime >= start && t.StartDateTime <= end).ToList();

        //        // Generisanje svih mogućih termina u zadanom periodu
        //        var allTermini = new List<Termin>();
        //        for (var date = start; date <= end; date = date.AddHours(1))
        //        {
        //            // Proveravamo da li je termin već zakazan
        //            var existingTermin = termini.FirstOrDefault(t => t.StartDateTime == date);
        //            if (existingTermin == null)
        //            {
        //                // Ako termin nije zakazan, dodajemo nezakazani termin
        //                allTermini.Add(new Termin
        //                {
        //                    StartDateTime = date,
        //                    EndDateTime = date.AddHours(1),
        //                    TerenId = idTeren,
        //                    User = null // Oznaka da je termin nezakazan
        //                });
        //            }
        //            else
        //            {
        //                // Ako je termin zakazan, dodajemo ga
        //                allTermini.Add(existingTermin);
        //            }
        //        }

        //        termini = allTermini;
        //    }

        //    // Sada `termini` sadrži sve termine, zakazane i nezakazane

        //    return Ok(termini);
        //}


        // GET: api/Termins/all/5
        [HttpGet("all/{idUser}")]
        public async Task<IActionResult> GetAll(int idUser)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Lista korisnika je NULL.",
                    Code = ErrorEnumeration.IsNull,
                    Action = "Get"
                });
            }

            var userDB = await _context.Users.FindAsync(idUser);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Traženi korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            var termini = _context.Termins.Where(u => u.UserId == idUser);
            if (termini == null || !termini.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Ne postoje termini za traženog korisnika.",
                    Code = ErrorEnumeration.IsEmpty,
                    Action = "Get"
                });
            }

            return Ok(await termini.ToListAsync());
        }

        // GET: api/Termins/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {

            var termin = await _context.Termins.Include(t => t.Teren).SingleOrDefaultAsync(t => t.Id == id);
            if (termin == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Ne postoji termin u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            return Ok(termin);
        }

        // POST: api/Termins/zakazi
        [HttpPost("zakazi")]
        public async Task<IActionResult> Post(TerminRequest terminRequest)
        {
            try {
                terminRequest.StartDateTime = TimeZoneInfo.ConvertTime(terminRequest.StartDateTime,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                var terminDB = await _context.Termins.FirstOrDefaultAsync(t => t.StartDateTime.Date == terminRequest.StartDateTime.Date &&
                t.StartDateTime.Hour == terminRequest.StartDateTime.Hour);

                if(terminDB != null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Termin je već zauzet.",
                        Code = ErrorEnumeration.BadRequest,
                        Action = "Post"
                    });
                }

                if (terminRequest.EndDateTime != null)
                {
                    terminRequest.EndDateTime = TimeZoneInfo.ConvertTime(terminRequest.EndDateTime.Value,
                            TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
                }
                var userDB = await _context.Users.FindAsync(terminRequest.UserId);

                if (userDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji član kome se zakazuje termin.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "Post"
                    });
                }

                var klubDB = await _context.Klubs.FindAsync(userDB.KlubId);
                if (klubDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji klub usera koji zakazuje termin.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "Post"
                    });
                }

                var neplaceniTermini = _context.Termins.Where(t => t.UserId != null &&
                t.UserId == userDB.Id &&
                t.Price != t.Placeno);

                if (neplaceniTermini.Any())
                {
                    var prviNeplacenTermin = await neplaceniTermini.MinAsync(n => n.StartDateTime);
                    int dani = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")).Subtract(prviNeplacenTermin).Days;

                    if (dani >= klubDB.DanaValute)
                    {
                        return NotFound(new ErrorResponse
                        {
                            Controller = "TerminsController",
                            Message = "Ne možete da rezervišete termin jer niste izmirili vaš dug!",
                            Code = ErrorEnumeration.BadRequest,
                            Action = "Post"
                        });
                    }
                }

                try
                {
                    var startTermin = new TimeOnly(terminRequest.StartDateTime.Hour, terminRequest.StartDateTime.Minute);

                    int terminType = (int)UserEnumeration.Vanredni;

                    if (userDB.Type == (int)UserEnumeration.Moderator ||
                        userDB.Type == (int)UserEnumeration.Radnik)
                    {
                        terminType = (int)UserEnumeration.Neclanski;
                    }

                    var naplataTermina = await _context.Naplataterminas.FirstOrDefaultAsync(n => n.Id == terminType);

                    if (naplataTermina == null)
                    {
                        return NotFound(new ErrorResponse
                        {
                            Controller = "TerminsController",
                            Message = "Nije definisana cena za navedeni tip korisnika.",
                            Code = ErrorEnumeration.NotFound,
                            Action = "Post"
                        });
                    }

                    decimal cenaTermina = naplataTermina.Price;

                    if (userDB.Type == (int)UserEnumeration.Plivajuci &&
                        userDB.FreeTermin > 0)
                    {
                        cenaTermina = 0;
                        userDB.FreeTermin--;

                        _context.Users.Update(userDB);

                        terminType = (int)UserEnumeration.Plivajuci;
                    }

                    var popustTermina = await _context.Popustiterminas.FirstOrDefaultAsync(p => p.TypeUser == userDB.Type);

                    if (popustTermina != null)
                    {
                        cenaTermina = Decimal.Round(cenaTermina * ((100 - popustTermina.Popust) / 100), 2);
                    }

                    terminDB = new Termin()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TerenId = terminRequest.TerenId,
                        UserId = terminRequest.UserId,
                        Type = terminType,
                        DateRezervacije = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")),
                        StartDateTime = terminRequest.StartDateTime,
                        EndDateTime = terminRequest.EndDateTime == null ? terminRequest.StartDateTime.AddHours(1) : terminRequest.EndDateTime,
                        Price = cenaTermina,
                        Placeno = terminRequest.Zaduzi == 1 ? 0 : cenaTermina
                    };

                    await _context.Termins.AddAsync(terminDB);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Conflict(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = $"{ex.Message}",
                        Code = ErrorEnumeration.Exception,
                        Action = "Post"
                    });
                }
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Post"
                });
            }

            return Ok();
        }

        // DELETE: api/Termins/3/id
        [HttpDelete("{idUser}/{id}")]
        public async Task<IActionResult> Otkazivanje(int idUser, string id)
        {
            var userDB = await _context.Users.FindAsync(idUser);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Ne postoji član koji otkazuje termin.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Otkazivanje"
                });
            }
            var terminDB = await _context.Termins.FindAsync(id);

            if (terminDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Ne postoji termin koji se otkazuje.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Otkazivanje"
                });
            }

            if (terminDB.UserId != idUser &&
                userDB.Type != (int)UserEnumeration.Moderator &&
                userDB.Type != (int)UserEnumeration.Radnik)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Ne možete da otkažete tuđi termin.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "Otkazivanje"
                });
            }

            var vremeDoTermina = terminDB.StartDateTime.Subtract(TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")));

            if (vremeDoTermina.Ticks < new TimeSpan(5, 30, 0).Ticks &&
                userDB.Type != (int)UserEnumeration.Moderator &&
                userDB.Type != (int)UserEnumeration.Radnik)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = "Prošlo je vreme za otkazivanje termina.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "Otkazivanje"
                });
            }

            try
            {
                _context.Termins.Remove(terminDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Otkazivanje"
                });
            }

            return Ok();
        }

        private bool TerminExists(string id)
        {
            return (_context.Termins?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }

}
