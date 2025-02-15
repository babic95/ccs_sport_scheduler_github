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
using CcsSportScheduler_API.Models.Requests.Uplata;
using Microsoft.VisualBasic;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UplatasController : ControllerBase
    {
        private readonly SportSchedulerContext _context;

        public UplatasController(SportSchedulerContext context)
        {
            _context = context;
        }

        // GET: api/all/Uplatas/5
        [Route("all/{idUser}")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int idUser)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UplatasController",
                    Message = "Lista korisnika je NULL.",
                    Code = ErrorEnumeration.IsNull,
                    Action = "GetAll"
                });
            }

            var userDB = await _context.Users.FindAsync(idUser);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UplatasController",
                    Message = "Traženi korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "GetAll"
                });
            }

            var uplate = _context.Uplata.Where(u => u.UserId == idUser);
            if (uplate == null ||
                !uplate.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UplatasController",
                    Message = "Ne postoje uplate za traženog korisnika.",
                    Code = ErrorEnumeration.IsEmpty,
                    Action = "GetAll"
                });
            }

            return Ok(uplate);
        }
        // GET: api/Uplatas/5
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var uplata = await _context.Uplata.FindAsync(id);
            if (uplata == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UplatasController",
                    Message = "Ne postoji uplata u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            return Ok(uplata);
        }

        // POST: api/Uplatas
        [HttpPost]
        public async Task<IActionResult> Post(UplataRequest uplataRequest)
        {
            if (await _context.Users.FindAsync(uplataRequest.UserId) == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UplatasController",
                    Message = "Ne postoji član kome se dodajte uplata.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Post"
                });
            }

            try
            {
                Uplata uplata = new Uplata()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = uplataRequest.UserId,
                    Date = TimeZoneInfo.ConvertTime(uplataRequest.Date,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")),
                    TotalAmount = uplataRequest.TotalAmount,
                    TypeUplata = uplataRequest.TypeUplata,
                    Razduzeno = 0
                };

                var termini = _context.Termins.Where(t => t.UserId == uplataRequest.UserId &&
                t.Placeno != t.Price);

                var racuni = _context.Racuns.Where(r => r.UserId == uplataRequest.UserId &&
                r.Placeno != r.TotalAmount);

                DateTime odDatuma = TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
                DateTime doDatuma = TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                if (termini != null &&
                    termini.Any())
                {
                    odDatuma = await termini.MinAsync(t => t.StartDateTime);
                    doDatuma = await termini.MaxAsync(t => t.StartDateTime);
                }

                if (racuni != null &&
                    racuni.Any())
                {
                    DateTime minDateRacuni = await racuni.MinAsync(r => r.Date);
                    DateTime maxDateRacuni = await racuni.MaxAsync(r => r.Date);

                    if (minDateRacuni < odDatuma)
                    {
                        odDatuma = minDateRacuni;
                    }
                    if(maxDateRacuni > doDatuma)
                    {
                        doDatuma = maxDateRacuni;
                    }
                }

                decimal uplataTotal = uplataRequest.TotalAmount;

                for (var date = odDatuma; date <= doDatuma; date = date.AddDays(1)) 
                {
                    if (termini != null &&
                        termini.Any())
                    {
                        var terminiDate = termini.Where(t => t.StartDateTime.Date == date.Date);

                        if (terminiDate.Any())
                        {
                            foreach (var t in terminiDate.OrderBy(o => o.StartDateTime))
                            {
                                if (uplataTotal > 0)
                                {
                                    decimal zaUplatu = t.Price - t.Placeno;

                                    if (zaUplatu >= uplataTotal)
                                    {
                                        t.Placeno += uplataTotal;
                                        uplata.Razduzeno += uplataTotal;

                                        if (uplata.TypeUplata == (int)UplataEnumeration.Poklon)
                                        {
                                            t.Otpis += uplataTotal;
                                        }

                                        uplataTotal = 0;
                                    }
                                    else
                                    {
                                        t.Placeno += zaUplatu;
                                        uplataTotal -= zaUplatu;
                                        uplata.Razduzeno += zaUplatu;

                                        if (uplata.TypeUplata == (int)UplataEnumeration.Poklon)
                                        {
                                            t.Otpis += zaUplatu;
                                        }
                                    }

                                    _context.Termins.Update(t);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (uplataTotal > 0)
                    {

                        if (racuni != null &&
                            racuni.Any())
                        {
                            var racuniDate = racuni.Where(r => r.Date.Date == date.Date);

                            if (racuniDate != null &&
                                racuniDate.Any())
                            {

                                foreach (var r in racuniDate.OrderBy(o => o.Date))
                                {
                                    if (uplataTotal > 0)
                                    {
                                        decimal zaUplatu = r.TotalAmount - r.Placeno;

                                        if (zaUplatu >= uplataTotal)
                                        {
                                            r.Placeno += uplataTotal;
                                            uplata.Razduzeno += uplataTotal;

                                            if (uplata.TypeUplata == (int)UplataEnumeration.Poklon)
                                            {
                                                r.Otpis += uplataTotal;
                                            }

                                            uplataTotal = 0;
                                        }
                                        else
                                        {
                                            r.Placeno += zaUplatu;
                                            uplataTotal -= zaUplatu;
                                            uplata.Razduzeno += zaUplatu;

                                            if (uplata.TypeUplata == (int)UplataEnumeration.Poklon)
                                            {
                                                r.Otpis += zaUplatu;
                                            }
                                        }

                                        _context.Racuns.Update(r);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                await _context.Uplata.AddAsync(uplata);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UplatasController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Post"
                });
            }

            return Ok();
        }

        private bool UplataExists(string id)
        {
          return (_context.Uplata?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
