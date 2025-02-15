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
using CcsSportScheduler_API.Models.Requests.Obavestenja;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CcsSportScheduler_API.Models.Response.Obavestenja;
using System.Collections;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObavestenjasController : Controller
    {
        private readonly SportSchedulerContext _context;

        public ObavestenjasController(SportSchedulerContext context)
        {
            _context = context;
        }

        // GET: api/Obavestenjas/Pregledano/{idUser}
        [Route("pregledano/{idUser}")]
        [HttpGet]
        public async Task<IActionResult> GetPregledano(int idUser)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "ObavestenjasController",
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
                    Controller = "ObavestenjasController",
                    Message = "Traženi korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            var obavestenjaDB = _context.Obavestenjas.Where(o => o.UserId == idUser &&
            o.Seen == 1);

            if (obavestenjaDB == null ||
                !obavestenjaDB.Any())
            {
                return Ok(null);
            }

            obavestenjaDB = obavestenjaDB.OrderBy(o => o.Date);

            List<ObavestenjeResponse> obavestenja = new List<ObavestenjeResponse>();

            await obavestenjaDB.ForEachAsync(o =>
            {
                obavestenja.Add(new ObavestenjeResponse(o));
            });

            return Ok(obavestenja);
        }

        // GET: api/Obavestenjas/Nepregledano/{idUser}
        [Route("nepregledan/{idUser}")]
        [HttpGet]
        public async Task<IActionResult> GetNepregledano(int idUser)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "ObavestenjasController",
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
                    Controller = "ObavestenjasController",
                    Message = "Traženi korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Get"
                });
            }

            var obavestenjaDB = _context.Obavestenjas.Where(o => o.UserId == idUser &&
            o.Seen == 0);

            if (obavestenjaDB == null ||
                !obavestenjaDB.Any())
            {
                return Ok(null);
            }

            obavestenjaDB = obavestenjaDB.OrderBy(o => o.Date);

            List<ObavestenjeResponse> obavestenja = new List<ObavestenjeResponse>();

            await obavestenjaDB.ForEachAsync(o =>
            {
                obavestenja.Add(new ObavestenjeResponse(o));
            });

            return Ok(obavestenja);
        }

        // POST: api/Obavestenjas
        [HttpPost]
        public async Task<IActionResult> Post(ObavestenjeRequest obavestenjeRequest)
        {
            var userDB = await _context.Users.FindAsync(obavestenjeRequest.UserId);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "ObavestenjasController",
                    Message = "Korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Post"
                });
            }

            if (!string.IsNullOrEmpty(obavestenjeRequest.TerminId))
            {
                var termin = await _context.Termins.FindAsync(obavestenjeRequest.TerminId);

                if (termin == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Controller = "ObavestenjasController",
                        Message = "Ne možete da pošaljete obaveštenje za termin koji je obrisan.",
                        Code = ErrorEnumeration.BadRequest,
                        Action = "Post"
                    });
                }
            }

            try
            {
                Obavestenja obavestenja = new Obavestenja()
                {
                    Id = Guid.NewGuid().ToString(),
                    Date = TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")),
                    PrvoSlanje = (int)ObavestenjaEnumeration.Ostalo,
                    TerminId = obavestenjeRequest.TerminId,
                    UserId = obavestenjeRequest.UserId,
                    Description = Encoding.UTF8.GetBytes(obavestenjeRequest.Description),
                };

                await _context.Obavestenjas.AddAsync(obavestenja);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "ObavestenjasController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Post"
                });
            }

            return Ok();
        }

        // POST: api/Obavestenjas/Seen/id
        [Route("seen/{id}")]
        [HttpPost]
        public async Task<IActionResult> Seen(string id)
        {
            var obavestenjaDB = await _context.Obavestenjas.FindAsync(id);

            if (obavestenjaDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "ObavestenjasController",
                    Message = "Ne možete da pregledate obaveštenje koje ne postoji u bazi.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "Post"
                });
            }

            try
            {
                obavestenjaDB.Seen = 1;

                _context.Obavestenjas.Update(obavestenjaDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "ObavestenjasController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Post"
                });
            }

            ObavestenjeResponse obavestenjeResponse = new ObavestenjeResponse(obavestenjaDB){};

            return Ok(obavestenjeResponse);
        }

        private bool ObavestenjaExists(string id)
        {
          return (_context.Obavestenjas?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
