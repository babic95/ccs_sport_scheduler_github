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

                Racun racunDB = new Racun()
                {
                    Id = Guid.NewGuid().ToString(),
                    InvoiceNumber = $"{userDB.Jmbg}-{userDB.Id}-{totalInvoices}",
                    Date = racunRequest.Date == null ? TimeZoneInfo.ConvertTime(DateTime.Now, 
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")) : racunRequest.Date.Value,
                    Racunitems = new List<RacunItem>(),
                    TotalAmount = racunRequest.TotalAmount,
                    UserId = userDB.Id,
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

        private bool RacunExists(string id)
        {
          return (_context.Racuns?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
