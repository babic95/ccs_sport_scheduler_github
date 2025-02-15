using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using CcsSportScheduler_API.Models.Response;
using CcsSportScheduler_API.Enumeration;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using CcsSportScheduler_API.Models.Requests.Klub;
using CcsSportScheduler_API.Models.Requests.Teren;
using System.Runtime.Intrinsics.X86;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KlubsController : Controller
    {
        private readonly SportSchedulerContext _context;

        public KlubsController(SportSchedulerContext context)
        {
            _context = context;
        }


        // GET: api/Klubs
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (_context.Klubs == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Lista klubova je NULL.",
                    Code = ErrorEnumeration.IsNull,
                    Action = "Get"
                });
            }

            return Ok(await _context.Klubs.ToListAsync());
        }

        // POST: api/Klubs
        [HttpPost]
        public async Task<IActionResult> Post(KlubRequest klub)
        {
            var klubDB = await _context.Klubs.FirstOrDefaultAsync(u => u.Pib == klub.Pib);

            if (klubDB != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub je već registrovan.",
                    Code = ErrorEnumeration.AlreadyExists,
                    Action = "Post",
                });
            }

            try
            {
                klubDB = new Klub()
                {
                    Address = klub.Address,
                    City = klub.City,
                    Email = klub.Email,
                    Name = klub.Name,
                    Number = klub.Number,
                    Pib = klub.Pib,
                };

                await _context.Klubs.AddAsync(klubDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Post",
                });
            }

            return Ok();
        }

        // PUT: api/Klubs/5
        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] KlubRequest klub)
        {
            var klubDB = await _context.Klubs.FindAsync(id);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Put",
                });
            }

            if (klub.Id != id)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"Ne možete da promenite ID kluba.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "Put",
                });
            }

            try
            {
                klubDB.Address = klub.Address;
                klubDB.City = klub.City;
                klubDB.Email = klub.Email;
                klubDB.Name = klub.Name;
                klubDB.Number = klub.Number;
                klubDB.Pib = klub.Pib;

                _context.Klubs.Update(klubDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Put",
                });
            }

            return Ok(klub);
        }

        // Get: api/Klubs/Teren/2
        [Route("teren/{idKlub}")]
        [HttpPost]
        public async Task<IActionResult> GetTeren(int idKlub)
        {
            var klubDB = await _context.Klubs.FindAsync(idKlub);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "GetTeren",
                });
            }

            var tereni = await _context.Terens.Where(t => t.KlubId == idKlub).ToListAsync();

            return Ok(tereni);
        }
        // POST: api/Klubs/Teren
        [Route("teren")]
        [HttpPost]
        public async Task<IActionResult> PostTeren(TerenRequest teren)
        {
            var klubDB = await _context.Klubs.FindAsync(teren.KlubId);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PostTeren",
                });
            }

            try
            {
                Teren terenDB = new Teren()
                {
                    KlubId = teren.KlubId,
                    Name = teren.Name,
                };

                await _context.Terens.AddAsync(terenDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PostTeren",
                });
            }

            return Ok();
        }

        // PUT: api/Klubs/Teren/2
        [Route("teren/{idTeren}")]
        [HttpPut]
        public async Task<IActionResult> PutTeren(int idTeren, [FromBody] TerenRequest teren)
        {
            if (idTeren != teren.Id)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Ne možete da promenite id terena.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PutTeren",
                });
            }

            var terenDB = await _context.Terens.FindAsync(idTeren);

            if (terenDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Teren ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutTeren",
                });
            }

            var klubDB = await _context.Klubs.FindAsync(teren.KlubId);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutTeren",
                });
            }

            try
            {
                terenDB.Name = teren.Name;

                _context.Terens.Update(terenDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PutTeren",
                });
            }

            return Ok();
        }

        // Get: api/klubs/naplataTermina/{idKlub}
        [Route("naplataTermina/{idKlub}")]
        [HttpGet]
        public async Task<IActionResult> GetNaplataTermina(int idKlub)
        {
            var klubDB = await _context.Klubs.FindAsync(idKlub);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "GetNaplataTermina",
                });
            }

            var cenaTermina = _context.Naplataterminas.Where(n => n.KlubId == idKlub);

            if (cenaTermina == null || 
                !cenaTermina.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"Ne postoje cene za termine.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "GetNaplataTermina",
                });
            }

            return Ok(await cenaTermina.ToListAsync());
        }

        // POST: api/Klubs/NaplataTermina
        [Route("naplataTermina")]
        [HttpPost]
        public async Task<IActionResult> PostNaplataTermina(NaplataTerminaRequest naplataTermina)
        {
            var klubDB = await _context.Klubs.FindAsync(naplataTermina.KlubId);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PostNaplataTermina",
                });
            }

            var naplataTerminaDB = await _context.Naplataterminas.FirstOrDefaultAsync(n =>
            (n.StartTime < naplataTermina.StartTime && n.EndTime > naplataTermina.StartTime) ||
            (n.StartTime < naplataTermina.EndTime && n.EndTime > naplataTermina.EndTime));

            if (naplataTerminaDB != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"Uneti termin se poklapa sa postojećim - {naplataTerminaDB.Name}",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PostNaplataTermina",
                });
            }

            try
            {
                naplataTerminaDB = new NaplataTermina()
                {
                    KlubId = naplataTermina.KlubId,
                    Name = naplataTermina.Name,
                    Price = naplataTermina.Price,
                    Vikend = naplataTermina.Vikend,
                    StartTime = naplataTermina.StartTime,
                    EndTime = naplataTermina.EndTime,
                };

                await _context.Naplataterminas.AddAsync(naplataTerminaDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PostNaplataTermina",
                });
            }

            return Ok();
        }

        // PUT: api/Klubs/NaplataTermina/2
        [Route("naplataTermina/{idNaplataTermina}")]
        [HttpPut]
        public async Task<IActionResult> PutNaplataTermina(int idNaplataTermina, [FromBody] NaplataTerminaRequest naplataTermina)
        {
            if (idNaplataTermina != naplataTermina.Id)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Ne možete da promenite id Naplate Termina.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PutNaplataTermina",
                });
            }

            var naplataTerminaDB = await _context.Naplataterminas.FindAsync(idNaplataTermina);

            if (naplataTerminaDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Naplata Termina ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutNaplataTermina",
                });
            }

            var ntDB = await _context.Naplataterminas.FirstOrDefaultAsync(n => n.Id != idNaplataTermina &&
            ((n.StartTime < naplataTermina.StartTime && n.EndTime > naplataTermina.StartTime) ||
            (n.StartTime < naplataTermina.EndTime && n.EndTime > naplataTermina.EndTime)));

            if (ntDB != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"Uneti termin se poklapa sa postojećim - {ntDB.Name}",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PutNaplataTermina",
                });
            }

            var klubDB = await _context.Klubs.FindAsync(naplataTermina.KlubId);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutNaplataTermina",
                });
            }

            try
            {
                naplataTerminaDB.Price = naplataTermina.Price;
                naplataTerminaDB.Vikend = naplataTermina.Vikend;
                naplataTerminaDB.StartTime = naplataTermina.StartTime;
                naplataTerminaDB.EndTime = naplataTermina.EndTime;
                naplataTerminaDB.Name = naplataTermina.Name;
                
                _context.Naplataterminas.Update(naplataTerminaDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PutNaplataTermina",
                });
            }

            return Ok();
        }

        // POST: api/Klubs/PopustiTermina
        [Route("popustiTermina")]
        [HttpPost]
        public async Task<IActionResult> PostPopustiTermina(PopustiTerminaRequest popustiTermina)
        {
            var klubDB = await _context.Klubs.FindAsync(popustiTermina.KlubId);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PostPopustiTermina",
                });
            }

            var popustiTerminaDB = await _context.Popustiterminas.FirstOrDefaultAsync(p => p.TypeUser == popustiTermina.TypeUser);

            if(popustiTerminaDB != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Već postoji popust za zadat tip člana.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PostPopustiTermina",
                });
            }

            try
            {
                popustiTerminaDB = new PopustiTermina()
                {
                    KlubId = popustiTermina.KlubId,
                    Name = popustiTermina.Name,
                    Popust = popustiTermina.Popust,
                    TypeUser = popustiTermina.TypeUser,
                };

                await _context.Popustiterminas.AddAsync(popustiTerminaDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PostPopustiTermina",
                });
            }

            return Ok();
        }

        // PUT: api/Klubs/PopustiTermina/2
        [Route("popustiTermina/{idPopustiTermina}")]
        [HttpPut]
        public async Task<IActionResult> PutPopustiTerminan(int idPopustiTermina, [FromBody] PopustiTerminaRequest popustiTermina)
        {
            if (idPopustiTermina != popustiTermina.Id)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Ne možete da promenite id Popusta.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PutPopustiTerminan",
                });
            }

            var popustiTerminaDB = await _context.Popustiterminas.FindAsync(idPopustiTermina);

            if (popustiTerminaDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Popust Termina ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutPopustiTerminan",
                });
            }

            var ptDB = await _context.Popustiterminas.FirstOrDefaultAsync(p => p.Id != idPopustiTermina &&
            p.TypeUser == popustiTermina.TypeUser);

            if (ptDB != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Već postoji popust za zadat tip člana.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PutPopustiTerminan",
                });
            }

            var klubDB = await _context.Klubs.FindAsync(popustiTermina.KlubId);

            if (klubDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = "Klub ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutPopustiTerminan",
                });
            }

            try
            {
                popustiTerminaDB.Name = popustiTermina.Name;
                popustiTerminaDB.Popust = popustiTermina.Popust;
                popustiTerminaDB.TypeUser = popustiTermina.TypeUser;

                _context.Popustiterminas.Update(popustiTerminaDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "KlubsController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PutPopustiTerminan",
                });
            }

            return Ok();
        }

        private bool KlubExists(int id)
        {
          return (_context.Klubs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
