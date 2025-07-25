﻿using System;
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
using CcsSportScheduler_API.Models.Requests.Zaduzenje;
using CcsSportScheduler_API.Models.Response.FinancialCard;

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

                //// Dohvati cene termina sa API-ja
                //var client = _httpClientFactory.CreateClient("MyHttpClient");
                //var responseCenaTermina = await client.GetAsync($"/api/klubs/naplataTermina/{idUser}");

                //if (!responseCenaTermina.IsSuccessStatusCode)
                //{
                //    return StatusCode((int)responseCenaTermina.StatusCode, new ErrorResponse
                //    {
                //        Controller = "TerminsController",
                //        Message = "Greška prilikom dohvatanja cena termina.",
                //        Code = ErrorEnumeration.NotFound,
                //        Action = "ZakazaniTermini"
                //    });
                //}

                //var cenaTermina = await responseCenaTermina.Content.ReadFromJsonAsync<NaplataTermina>();

                if (!startDate.HasValue)
                {
                    startDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
                }
                else
                {
                    startDate = TimeZoneInfo.ConvertTime(startDate.Value,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
                }
                DateTime start = new DateTime(startDate.Value.Year, startDate.Value.Month, startDate.Value.Day, 7, 0, 0);

                DateTime end = start.AddDays(6).AddHours(16);

                // Filtriranje termina u zadanom periodu
                var termini = _context.Termins.Where(t => t.TerenId == idTeren &&
                t.StartDateTime >= start && t.StartDateTime <= end).Include(t => t.User).ToList();

                // Generisanje svih mogućih termina u zadanom periodu
                var allTermini = new List<Termin>();

                NaplataTermina? cenaTermina = null;
                DateTime? lastDate = null;

                for (var date = start; date <= end; date = date.AddHours(1))
                {
                    if (date.Hour < 7 || date.Hour > 22)
                    {
                        continue; // Preskoči termine van intervala 7-22h
                    }

                    if (lastDate == null ||
                        lastDate.Value.Date != date.Date)
                    {
                        lastDate = date.Date;
                        if (userDB.Type == (int)UserEnumeration.Plivajuci)
                        {
                            DateTime startForNaplata = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
                            DateTime endForNaplata = startForNaplata.AddMonths(1).AddDays(-1);

                            var freeTermins = _context.Termins.Where(t => t.UserId == userDB.Id &&
                            t.StartDateTime.Date >= startForNaplata &&
                            t.StartDateTime.Date <= endForNaplata &&
                            t.Price == 0);

                            if (freeTermins != null &&
                                freeTermins.Count() <= 3)
                            {
                                cenaTermina = _context.Naplataterminas.FirstOrDefault(n => n.Id == (int)TerminEnumeration.Free);
                            }
                            else
                            {
                                cenaTermina = _context.Naplataterminas.FirstOrDefault(n => n.Id == userDB.Type);
                            }
                        }
                        else
                        {
                            cenaTermina = _context.Naplataterminas.FirstOrDefault(n => n.Id == userDB.Type);
                        }

                        if (cenaTermina == null)
                        {
                            return BadRequest(new ErrorResponse
                            {
                                Controller = "TerminsController",
                                Message = $"Ne postoje cene za termine.",
                                Code = ErrorEnumeration.BadRequest,
                                Action = "ZakazaniTermini",
                            });
                        }
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
                            Price = cenaTermina == null ? 600 : cenaTermina.Price, // Postavljanje cene za nezakazane termine
                            Type = -1 // Oznaka da je termin nezakazan
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
            try
            {
                terminRequest.StartDateTime = TimeZoneInfo.ConvertTime(terminRequest.StartDateTime,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                var terminDB = await _context.Termins.FirstOrDefaultAsync(t => t.StartDateTime.Date == terminRequest.StartDateTime.Date &&
                t.StartDateTime.Hour == terminRequest.StartDateTime.Hour &&
                t.TerenId == terminRequest.TerenId);

                if (terminDB != null)
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
                (t.Price != t.Placeno + t.Otpis));

                if (neplaceniTermini.Any())
                {
                    var prviNeplacenTermin = await neplaceniTermini.MinAsync(n => n.StartDateTime);
                    int dani = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")).Subtract(prviNeplacenTermin).Days;

                    if (dani >= klubDB.DanaValute + userDB.Valuta)
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
                else
                {
                    var neplacenaZaduzenja = _context.Zaduzenja.Where(z => z.UserId == userDB.Id &&
                    z.TotalAmount != z.Otpis + z.Placeno);

                    if (neplacenaZaduzenja.Any())
                    {
                        var prviNeplacenZaduzenje = await neplacenaZaduzenja.MinAsync(n => n.Date);
                        int dani = TimeZoneInfo.ConvertTime(DateTime.Now,
                            TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")).Subtract(prviNeplacenZaduzenje).Days;
                        if (dani >= klubDB.DanaValute + userDB.Valuta)
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
                }

                try
                {
                    var startTermin = new TimeOnly(terminRequest.StartDateTime.Hour, terminRequest.StartDateTime.Minute);

                    int terminType = userDB.Type;

                    if (userDB.Type == (int)UserEnumeration.Moderator ||
                        userDB.Type == (int)UserEnumeration.Radnik)
                    {
                        terminType = (int)UserEnumeration.Neclanski;
                    }
                    else if(userDB.Type == (int)UserEnumeration.Fiksni ||
                        userDB.Type == (int)UserEnumeration.Plivajuci)
                    {
                        terminType = (int)UserEnumeration.Vanredni;
                    }
                    //else if (terminRequest.Zaduzi == 0)
                    //{
                    //    terminType = userDB.Type;
                    //}

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

                    if (userDB.Type == (int)UserEnumeration.Plivajuci 
                        /*&& userDB.FreeTermin > 0*/)
                    {
                        DateTime date = TimeZoneInfo.ConvertTime(terminRequest.StartDateTime,
                            TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                        DateTime startDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
                        DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                        var freetermins = await _context.Termins.Where(t => t.UserId == userDB.Id &&
                        t.StartDateTime.Date >= startDate .Date && t.StartDateTime.Date <= endDate.Date).ToListAsync();

                        if(freetermins.Count < 4)
                        {
                            cenaTermina = 0;
                            //userDB.FreeTermin--;

                            //_context.Users.Update(userDB);

                            terminType = (int)UserEnumeration.Plivajuci;
                        }
                    }
                    else if(userDB.Type == (int)UserEnumeration.Trenerski)
                    {
                        terminType = (int)UserEnumeration.Trenerski;
                    }

                    var popustTermina = await _context.Popustiterminas.FirstOrDefaultAsync(p => p.TypeUser == userDB.Type);

                    if (popustTermina != null)
                    {
                        cenaTermina = Decimal.Round(cenaTermina * ((100 - popustTermina.Popust) / 100), 2);
                    }

                    decimal pretplata = 0;
                    if (terminRequest.Zaduzi == 1)
                    {
                        if (cenaTermina > 0)
                        {
                            pretplata = await GetPretplata(terminRequest.UserId, cenaTermina);

                            //if (pretplata > 0)
                            //{
                            //    if (pretplata >= cenaTermina)
                            //    {
                            //        placeno = cenaTermina;
                            //    }
                            //    else
                            //    {
                            //        placeno = pretplata;
                            //    }
                            //}
                        }
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
                        Price = terminRequest.Zaduzi == 1 ? cenaTermina : 0,
                        Placeno = pretplata
                    };

                    await _context.Termins.AddAsync(terminDB);

                    //if (placeno > 0)
                    //{
                    //    var pretplate = _context.Uplata.Where(p => p.UserId == terminRequest.UserId &&
                    //    p.Razduzeno != p.TotalAmount);

                    //    if (pretplate.Any())
                    //    {
                    //        foreach (var pretplata in pretplate)
                    //        {
                    //            if (placeno <= 0)
                    //            {
                    //                break;
                    //            }
                    //            if (pretplata.TotalAmount - pretplata.Razduzeno - placeno >= 0)
                    //            {
                    //                pretplata.Razduzeno += placeno;

                    //                placeno = 0;
                    //            }
                    //            else
                    //            {
                    //                placeno = (pretplata.Razduzeno + placeno) - pretplata.TotalAmount;
                    //                pretplata.Razduzeno = pretplata.TotalAmount;
                    //            }
                    //            _context.Uplata.Update(pretplata);
                    //        }
                    //    }
                    //}

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
        // POST: api/Termins/zakazi/fiksni
        [HttpPost("zakazi/fiksni")]
        public async Task<IActionResult> PostFiksni(TerminFiksniRequest terminRequest)
        {
            try
            {
                // Convert and validate dates
                for (int i = 0; i < terminRequest.Dates.Count; i++)
                {
                    var terminDB = await _context.Termins.FirstOrDefaultAsync(t => t.StartDateTime.Date == terminRequest.Dates[i].Date &&
                    t.StartDateTime.Hour == terminRequest.Dates[i].Hour &&
                    t.TerenId == terminRequest.TerenId);

                    if (terminDB != null)
                    {
                        return NotFound(new ErrorResponse
                        {
                            Controller = "TerminsController",
                            Message = "Termin je već zauzet.",
                            Code = ErrorEnumeration.BadRequest,
                            Action = "PostFiksni"
                        });
                    }
                }

                // Validate user
                var userDB = await _context.Users.FindAsync(terminRequest.UserId);
                if (userDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji član kome se zakazuje termin.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "PostFiksni"
                    });
                }

                // Validate club
                var klubDB = await _context.Klubs.FindAsync(userDB.KlubId);
                if (klubDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji klub usera koji zakazuje termin.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "PostFiksni"
                    });
                }

                // Add all terms to context
                var termini = new List<Termin>();
                foreach (var t in terminRequest.Dates)
                {
                    var terminDB = new Termin()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TerenId = terminRequest.TerenId,
                        UserId = terminRequest.UserId,
                        Type = userDB.Type,
                        DateRezervacije = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")),
                        StartDateTime = t,
                        EndDateTime = t.AddHours(1),
                        Price = 0,
                        Placeno = 0
                    };
                    termini.Add(terminDB);
                }

                // Bulk insert
                await _context.Termins.AddRangeAsync(termini);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PostFiksni"
                });
            }
        }
        // POST: api/Termins/otkazi/fiksni
        [HttpPost("otkazi/fiksni")]
        public async Task<IActionResult> PostOtkaziFiksni(TerminOtkazFiksniRequest terminRequest)
        {
            try
            {
                terminRequest.Date = TimeZoneInfo.ConvertTime(terminRequest.Date,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
                // Validate user
                var userDB = await _context.Users.FindAsync(terminRequest.UserId);
                if (userDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji član kome se otkazuje fiksni termin.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "PostOtkaziFiksni"
                    });
                }

                // Validate club
                var klubDB = await _context.Klubs.FindAsync(userDB.KlubId);
                if (klubDB == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "TerminsController",
                        Message = "Ne postoji klub usera koji okazuje fiksni termin.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "PostOtkaziFiksni"
                    });
                }

                // Add all terms to context
                var termini = new List<Termin>();

                var dates = GetDatesForDayOfWeek(terminRequest.Date,
                    terminRequest.Date.DayOfWeek);

                foreach (var t in dates)
                {
                    var terminDB = await _context.Termins.FirstOrDefaultAsync(termin => termin.TerenId == terminRequest.TerenId &&
                    termin.UserId == terminRequest.UserId &&
                    termin.StartDateTime == t &&
                    termin.Type == (int)TerminEnumeration.Fiksni); 

                    if(terminDB != null)
                    {
                        termini.Add(terminDB);
                    }
                }

                // Bulk insert
                _context.Termins.RemoveRange(termini);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "TerminsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PostOtkaziFiksni"
                });
            }
        }

        // DELETE: api/Termins/3/id
        [HttpDelete("{idUser}/{id}")]
        public async Task<IActionResult> Otkazivanje(int idUser, string id, [FromQuery] bool isFree = true)
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
                if (isFree &&
                    terminDB.UserId.HasValue &&
                    terminDB.Placeno > 0)
                {
                    Uplata uplata = new Uplata()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Date = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")),
                        TotalAmount = terminDB.Placeno,
                        Razduzeno = 0,
                        UserId = terminDB.UserId.Value,
                        TypeUplata = (int)UplataEnumeration.OtkazTermina,
                        SkidajSaStanja = 1,
                    };

                    await _context.Uplata.AddAsync(uplata);
                }

                if (terminDB.Type == (int)TerminEnumeration.Plivajuci)
                {
                    if (terminDB.StartDateTime.Month == DateTime.Now.Month &&
                        terminDB.StartDateTime.Year == DateTime.Now.Year)
                    {
                        var userPlivajuci = await _context.Users.FindAsync(terminDB.UserId);

                        if (userPlivajuci != null &&
                            userPlivajuci.Type == (int)UserEnumeration.Plivajuci)
                        {
                            {
                                userPlivajuci.FreeTermin++;
                                _context.Users.Update(userPlivajuci);
                            }
                        }
                    }
                }

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
        private async Task<decimal> GetPretplata(int userId, decimal racunAmount)
        {
            try
            {
                decimal pretplata = 0;

                DateTime from = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime to = new DateTime(DateTime.Now.Year, 12, 31);

                var uplate = await _context.Uplata.Where(u => u.UserId == userId &&
                u.TotalAmount - u.Razduzeno > 0 &&
                u.Date >= from && u.Date <= to &&
                u.SkidajSaStanja == 1).ToListAsync();

                if (uplate.Any())
                {
                    foreach (var uplata in uplate.OrderBy(u => u.Date))
                    {
                        if (racunAmount > 0)
                        {
                            if (uplata.TotalAmount - uplata.Razduzeno >= racunAmount)
                            {
                                uplata.Razduzeno += racunAmount;
                                pretplata = racunAmount;
                                racunAmount = 0;
                            }
                            else
                            {
                                racunAmount -= uplata.TotalAmount - uplata.Razduzeno;
                                pretplata += uplata.TotalAmount - uplata.Razduzeno;
                                uplata.Razduzeno = uplata.TotalAmount;
                            }
                            _context.Uplata.Update(uplata);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return pretplata;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        //private async Task<decimal> GetPretplata(int userId)
        //{
        //    try
        //    {
        //        decimal pretplata = 0;

        //        DateTime from = new DateTime(DateTime.Now.Year, 1, 1);
        //        DateTime to = new DateTime(DateTime.Now.Year, 12, 31);

        //        var clanarice = await GetAllClanarice(userId, from, to);
        //        var termini = await GetAllTermins(userId, from, to);
        //        var kafic = await GetAllKafic(userId, from, to);
        //        var prodavnica = await GetAllProdavnica(userId, from, to);
        //        var kotizacije = await GetAllKotizacija(userId, from, to);
        //        var otpisPozajmice = await GetAllOtpisPozajmice(userId, from, to);
        //        var uplate = await GetAllUplate(userId, from, to);
        //        var pokloni = await GetAllPoklon(userId, from, to);
        //        var pozajmica = await GetAllPozajmica(userId, from, to);
        //        var otkazTermina = await GetAllOtkazTermina(userId, from, to);

        //        var items = new List<FinancialCardItemResponse>();
        //        items.AddRange(clanarice);
        //        items.AddRange(termini);
        //        items.AddRange(kafic);
        //        items.AddRange(prodavnica);
        //        items.AddRange(kotizacije);
        //        items.AddRange(otpisPozajmice);
        //        items.AddRange(uplate);
        //        items.AddRange(pokloni);
        //        items.AddRange(pozajmica);
        //        items.AddRange(otkazTermina);

        //        decimal totalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate ||
        //                i.Type == FinancialCardTypeEnumeration.Poklon ||
        //                i.Type == FinancialCardTypeEnumeration.Pozajmica ||
        //                i.Type == FinancialCardTypeEnumeration.OtkazTermina).Sum(u => u.Razduzenje);

        //        decimal totalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kafic ||
        //        i.Type == FinancialCardTypeEnumeration.Termini ||
        //        i.Type == FinancialCardTypeEnumeration.Kotizacije ||
        //        i.Type == FinancialCardTypeEnumeration.Prodavnica ||
        //        i.Type == FinancialCardTypeEnumeration.OtpisPozajmice ||
        //        i.Type == FinancialCardTypeEnumeration.Clanarina).Sum(t => t.Zaduzenje);

        //        return totalRazduzenje - totalZaduzenje;
        //    }
        //    catch (Exception ex)
        //    {
        //        return 0;
        //    }
        //}
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
        private List<DateTime> GetDatesForDayOfWeek(DateTime startDate,
            DayOfWeek dayOfWeek)
        {
            List<DateTime> dates = new List<DateTime>();

            startDate = new DateTime(startDate.Year,
                startDate.Month,
                startDate.Day,
                startDate.Hour,
                0,
                0);
            DateTime endDate = new DateTime(startDate.Year, 10, 31, 23, 0, 0);

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
        private bool TerminExists(string id)
        {
            return (_context.Termins?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }

}
