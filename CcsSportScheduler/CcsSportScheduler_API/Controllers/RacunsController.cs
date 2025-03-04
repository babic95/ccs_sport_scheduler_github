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
using Humanizer;
using CcsSportScheduler_API.Models.Response.FinancialCard;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RacunsController : ControllerBase
    {
        private readonly SportSchedulerContext _context;

        public RacunsController(SportSchedulerContext context)
        {
            _context = context;
        }

        // GET: api/Racuns/5
        [Route("all/{idUser}")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int idUser)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "RacunsController",
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
                    Controller = "RacunsController",
                    Message = "Traženi korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "GetAll"
                });
            }

            var racuni = _context.Racuns.Where(r => r.UserId == idUser);
            if (racuni == null ||
                !racuni.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "RacunsController",
                    Message = "Ne postoje racuni za traženog korisnika.",
                    Code = ErrorEnumeration.IsEmpty,
                    Action = "GetAll"
                });
            }

            return Ok(racuni.Include(r => r.Racunitems).ToList());
        }

        // GET: api/Racuns/{id}
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var racun = await _context.Racuns.Include(t => t.Racunitems).SingleOrDefaultAsync(r => r.Id == id);
            if (racun == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "RacunsController",
                    Message = "Ne postoji racun u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            return Ok(racun);
        }

        // POST: api/Racuns
        [HttpPost]
        public async Task<IActionResult> Post(RacunRequest racunRequest)
        {
            var userDB = await _context.Users.FindAsync(racunRequest.UserId);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "RacunsController",
                    Message = "Ne postoji član kome se izdaje račun.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Post"
                });
            }

            if (!racunRequest.Items.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "RacunsController",
                    Message = "Račun mora da ima artikle.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "Post"
                });
            }

            try
            {
                var totalInvoices = _context.Racuns.Where(r => r.UserId == userDB.Id).Count();
                totalInvoices++;

                if(racunRequest.Placeno == 0)
                {
                    decimal pretplata = await GetPretplata(userDB.Id);

                    if (pretplata > 0)
                    {
                        if (pretplata >= racunRequest.TotalAmount)
                        {
                            racunRequest.Placeno = racunRequest.TotalAmount;
                        }
                        else
                        {
                            racunRequest.Placeno = pretplata;
                        }
                    }
                }

                Racun racunDB = new Racun()
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceNumber = $"{userDB.Jmbg}-{userDB.Id}-{totalInvoices}",
                    Date = racunRequest.Date == null ? TimeZoneInfo.ConvertTime(DateTime.Now, 
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")) : racunRequest.Date.Value,
                    Racunitems = new List<RacunItem>(),
                    TotalAmount = racunRequest.TotalAmount,
                    UserId = userDB.Id,
                    Placeno = racunRequest.Placeno,
                    Otpis = 0,
                    Type = racunRequest.Type,
                };

                foreach (var item in racunRequest.Items)
                {
                    var itemDB = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.ItemsId &&
                    i.KlubId == userDB.KlubId);

                    if(itemDB == null)
                    {
                        itemDB = new Item()
                        {
                            Id = item.ItemsId,
                            KlubId = userDB.KlubId,
                            Name = item.Name,
                            Price = item.UnitPrice,
                        };

                        await _context.Items.AddAsync(itemDB);
                    }
                    else
                    {
                        if(itemDB.Price != item.UnitPrice)
                        {
                            itemDB.Price = item.UnitPrice;

                            _context.Items.Update(itemDB);
                        }
                    }

                    RacunItem racunItemDB = new RacunItem()
                    {
                        ItemsId = item.ItemsId,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        RacunId = racunDB.Id,
                        TotalAmount = item.TotalAmount,
                        UnitPrice = item.UnitPrice,
                    };

                    racunDB.Racunitems.Add(racunItemDB);
                }

                await _context.SaveChangesAsync();

                await _context.Racuns.AddAsync(racunDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "RacunsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Post"
                });
            }

            return Ok();
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

                decimal totalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate ||
                        i.Type == FinancialCardTypeEnumeration.Poklon ||
                        i.Type == FinancialCardTypeEnumeration.Pozajmica).Sum(u => u.Razduzenje);

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
        private bool RacunExists(string id)
        {
          return (_context.Racuns?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
