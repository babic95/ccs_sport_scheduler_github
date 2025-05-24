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
using CcsSportScheduler_API.Models.Response.FinancialCard;
using CcsSportScheduler_API.Models.Requests.User;
using CcsSportScheduler_API.Models.Requests.Racun;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using CcsSportScheduler_API.Models.Response.User;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SportSchedulerContext _context;
        private readonly IAmazonS3 _s3Client;

        private const string _bucketName = "ccs";
        private const string _imageFolderName = "CcsSportScheduler";
        private readonly string _cdnEndpoint = "https://ccs.fra1.cdn.digitaloceanspaces.com"; // Pravi CDN endpoint

        public UsersController(SportSchedulerContext context, IAmazonS3 s3Client)
        {
            _context = context;
            _s3Client = s3Client;
        }

        // GET: api/Users/GetAllUsersFromKlub/5
        [Route("getAllUsersFromKlub/{idKlub}")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersFromKlub(int idKlub)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Lista korisnika je NULL.",
                    Code = ErrorEnumeration.IsNull,
                    Action = "GetAllUsersFromKlub"
                });
            }

            var klubDB = await _context.Klubs.FindAsync(idKlub);

            if (klubDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Traženi klub ne postoji.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "GetAllUsersFromKlub"
                });
            }

            var users = _context.Users.Where(u => u.KlubId == idKlub);
            if (users == null ||
                !users.Any())
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Ne postoje korisnici za traženi klub.",
                    Code = ErrorEnumeration.IsEmpty,
                    Action = "GetAllUsersFromKlub"
                });
            }

            return Ok(users);
        }

        // GET: api/Users/5
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (_context.Users == null)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Lista korisnika je NULL.",
                    Code = ErrorEnumeration.IsNull,
                    Action = "Get"
                });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Ne postoji korisnki u bazi podataka.",
                    Code = ErrorEnumeration.IsEmpty,
                    Action = "Get"
                });
            }

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser(UserRequest user)
        {
            if (await _context.Klubs.FindAsync(user.KlubId) == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Ne postoji klub u koji se dodaje korisnik.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PostUser"
                });
            }

            if (user.KlubId == null ||
                //user.Type == null ||
                string.IsNullOrEmpty(user.Password) ||
                string.IsNullOrEmpty(user.Username))
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Morate popuniti obavezna polja.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PostUser"
                });
            }

            var userDB = await _context.Users.FirstOrDefaultAsync(u => u.KlubId == user.KlubId &&
            (u.Jmbg == user.Jmbg || u.Username == user.Username));

            if (userDB != null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Korisnik je već registrovan.",
                    Code = ErrorEnumeration.AlreadyExists,
                    Action = "PostUser"
                });
            }

            try
            {
                userDB = new User()
                {
                    Year = user.Year,
                    Contact = user.Contact,
                    Email = user.Email,
                    FullName = user.FullName,
                    Jmbg = user.Jmbg,
                    KlubId = user.KlubId.Value,
                    Password = user.Password,
                    Username = user.Username,
                    Type = (int)UserEnumeration.Neclanski,// user.Type.Value,
                    Pol = user.Pol,
                };

                await _context.Users.AddAsync(userDB);
                await _context.SaveChangesAsync();

                return Ok(userDB);
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PostUser"
                });
            }
        }

        // POST: api/Users/Login
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            try
            {
                var userDB = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username &&
                u.Password == login.Password);

                if (userDB == null)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Controller = "UsersController",
                        Message = "Ne postoji korisnik.",
                        Code = ErrorEnumeration.NotFound,
                        Action = "Login"
                    });
                }

                return Ok(userDB);
            }
            catch(Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "Login"
                });
            }
        }
        // POST: api/Users/uploadProfileImage
        [HttpPost("uploadProfileImage")]
        public async Task<IActionResult> UploadProfileImage([FromForm] UploadImageRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                if (request.Image == null || request.Image.Length == 0)
                {
                    return BadRequest(new { Message = "Invalid file" });
                }

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = user.ProfileImageUrl.Replace($"{_cdnEndpoint}/", "")
                    };
                    await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                }

                var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
                if (!bucketExists)
                {
                    var bucketRequest = new PutBucketRequest()
                    {
                        BucketName = _bucketName,
                        UseClientRegion = true
                    };
                    await _s3Client.PutBucketAsync(bucketRequest);
                }

                string key = $"{_imageFolderName}/{user.Username}_{request.Image.FileName}";
                var objectRequest = new PutObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = request.Image.OpenReadStream(),
                    CannedACL = S3CannedACL.PublicRead // Postavljanje javnog pristupa
                };
                var response = await _s3Client.PutObjectAsync(objectRequest);

                if (response != null)
                {
                    user.ProfileImageUrl = $"{_cdnEndpoint}/{key}";
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { user.ProfileImageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        // PUT: api/Users/5
        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserRequest user)
        {
            var userDB = await _context.Users.FindAsync(id);

            if (userDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"Korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "PutUser"
                });
            }

            if (user.Id != id)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"Ne možete da promenite ID korisnika.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "PutUser"
                });
            }

            try
            {
                userDB.Contact = user.Contact;
                userDB.Email = user.Email;
                //userDB.Password = user.Password;
                //userDB.Username = user.Username;
                userDB.Year = user.Year;
                userDB.FullName = user.FullName;
                userDB.Jmbg = user.Jmbg;
                //userDB.Type = user.Type;

                _context.Users.Update(userDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "PutUser"
                });
            }

            return Ok(user);
        }

        // DELETE: api/Users/5
        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userDB = await _context.Users.FindAsync(id);

            if (userDB == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"Korisnik ne postoji u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "DeleteUser"
                });
            }

            try
            {
                _context.Users.Remove(userDB);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "DeleteUser"
                });
            }

            return Ok();
        }

        // POST: api/Users/ChangePassword/4
        [Route("changePassword/{id}")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest changePassword)
        {
            if (id != changePassword.IdUser)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Neautorizovano menjanje lozinke.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "ChangePassword"
                });
            }

            var userDB = await _context.Users.FindAsync(id);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Ne postoji korisnik u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "ChangePassword"
                });
            }

            if (userDB.Password != changePassword.OldPassword)
            {
                return BadRequest(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Nije tačna stara lozinka.",
                    Code = ErrorEnumeration.BadRequest,
                    Action = "ChangePassword"
                });
            }

            try
            {
                userDB.Password = changePassword.NewPassword;
                _context.Users.Update(userDB);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "ChangePassword"
                });
            }

            return Ok();
        }

        // Get: api/Users/FinancialCard/2
        [Route("financialCard/{id}/{pageNumber?}/{pageSize?}")]
        [HttpGet]
        public async Task<IActionResult> FinancialCard(int id, int pageNumber = 1, int pageSize = 20, string? fromDate = null, string? toDate = null, int? type = null)
        {
            var userDB = await _context.Users.FindAsync(id);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = "Ne postoji korisnik u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "FinancialCard"
                });
            }

            FinancialCardResponse financialCardResponse = new FinancialCardResponse()
            {
                Items = new List<FinancialCardItemResponse>(),
                TotalRazduzenje = 0,
                TotalZaduzenje = 0,
                TotalCount = 0,
            };

            try
            {
                DateTime from = fromDate != null ? TimeZoneInfo.ConvertTime(DateTime.Parse(fromDate),
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")) :
                    TimeZoneInfo.ConvertTime(new DateTime(DateTime.Now.Year, 1, 1),
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                DateTime to = toDate != null ? TimeZoneInfo.ConvertTime(DateTime.Parse(toDate),
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time")) :
                    TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                from = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0);
                to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);

                var clanarice = await GetAllClanarice(id, from, to);
                var termini = await GetAllTermins(id, from, to);
                var kafic = await GetAllKafic(id, from, to);
                var prodavnica = await GetAllProdavnica(id, from, to);
                var kotizacije = await GetAllKotizacija(id, from, to);
                var otpisPozajmice = await GetAllOtpisPozajmice(id, from, to);
                var uplate = await GetAllUplate(id, from, to);
                var pokloni = await GetAllPoklon(id, from, to);
                var pozajmica = await GetAllPozajmica(id, from, to);
                var otkazTermina = await GetAllOtkazTermina(id, from, to);

                var items = new List<FinancialCardItemResponse>();

                if (type == null ||
                    type == -1)
                {
                    items.AddRange(clanarice);
                    items.AddRange(termini);
                    items.AddRange(kafic);
                    items.AddRange(prodavnica);
                    items.AddRange(kotizacije);
                    items.AddRange(otpisPozajmice);
                    items.AddRange(uplate);
                    items.AddRange(pokloni);
                    items.AddRange(pozajmica);

                    //financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate ||
                    //i.Type == FinancialCardTypeEnumeration.Poklon ||
                    //i.Type == FinancialCardTypeEnumeration.Pozajmica ||
                    //i.Type == FinancialCardTypeEnumeration.OtkazTermina ||
                    //i.Type == FinancialCardTypeEnumeration.Kotizacije ||
                    //i.Type == FinancialCardTypeEnumeration.Kafic ||
                    //i.Type == FinancialCardTypeEnumeration.Prodavnica).Sum(u => u.Razduzenje);

                    //financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kafic ||
                    //i.Type == FinancialCardTypeEnumeration.Termini ||
                    //i.Type == FinancialCardTypeEnumeration.Kotizacije ||
                    //i.Type == FinancialCardTypeEnumeration.Prodavnica ||
                    //i.Type == FinancialCardTypeEnumeration.OtpisPozajmice ||
                    //i.Type == FinancialCardTypeEnumeration.Clanarina).Sum(t => t.Zaduzenje);

                    financialCardResponse.TotalRazduzenje = items.Sum(u => u.Razduzenje);

                    financialCardResponse.TotalZaduzenje = items.Sum(t => t.Zaduzenje);

                    items.AddRange(otkazTermina);
                }
                else
                {
                    if (type == (int)FinancialCardTypeEnumeration.Clanarina)
                    {
                        items.AddRange(clanarice);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Clanarina).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Clanarina).Sum(t => t.Zaduzenje);
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.Termini)
                    {
                        items.AddRange(termini);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Termini).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Termini).Sum(t => t.Zaduzenje);
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.Kafic)
                    {
                        items.AddRange(kafic);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kafic).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kafic).Sum(t => t.Zaduzenje);
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.Prodavnica)
                    {
                        items.AddRange(prodavnica);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Prodavnica).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Prodavnica).Sum(t => t.Zaduzenje);
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.Kotizacije)
                    {
                        items.AddRange(kotizacije);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kotizacije).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Kotizacije).Sum(t => t.Zaduzenje);
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.OtpisPozajmice)
                    {
                        items.AddRange(otpisPozajmice);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.OtpisPozajmice).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.OtpisPozajmice).Sum(t => t.Zaduzenje);
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.Uplate)
                    {
                        items.AddRange(uplate);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = 0;
                    }
                    else if(type == (int)FinancialCardTypeEnumeration.Poklon)
                    {
                        items.AddRange(pokloni);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Poklon).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = 0;
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.Pozajmica)
                    {
                        items.AddRange(pozajmica);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Pozajmica).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = 0;
                    }
                    else if (type == (int)FinancialCardTypeEnumeration.OtkazTermina)
                    {
                        items.AddRange(otkazTermina);

                        financialCardResponse.TotalRazduzenje = 0; // items.Where(i => i.Type == FinancialCardTypeEnumeration.OtkazTermina).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = 0;
                    }
                }

                financialCardResponse.TotalCount = items.Count;

                var placeni = items.Where(i => i.Razduzenje == i.Zaduzenje || i.Zaduzenje == i.Pretplata);
                var neplaceni = items.Except(placeni);

                if (neplaceni.Any())
                {
                    neplaceni = neplaceni.OrderByDescending(i => i.Date);
                }
                else
                {
                    neplaceni = new List<FinancialCardItemResponse>();
                }

                if (placeni.Any())
                {
                    placeni = placeni.OrderByDescending(i => i.Date);
                }
                else
                {
                    placeni = new List<FinancialCardItemResponse>();
                }

                var resultItems = new List<FinancialCardItemResponse>();
                resultItems.AddRange(neplaceni);
                resultItems.AddRange(placeni);

                financialCardResponse.Items = resultItems.Skip((pageNumber - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .ToList();
            }
            catch (Exception ex)
            {
                return Conflict(new ErrorResponse
                {
                    Controller = "UsersController",
                    Message = $"{ex.Message}",
                    Code = ErrorEnumeration.Exception,
                    Action = "FinancialCard"
                });
            }

            return Ok(financialCardResponse);
        }


        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
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
                            Pretplata = c.Placeno
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
                            Pretplata = t.Placeno,
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
                            Pretplata = r.Pretplata
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
                            Pretplata = r.Pretplata
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
                            Pretplata = 0
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
                            Razduzenje = u.TotalAmount - u.Razduzeno,
                            Zaduzenje = 0,
                            TotalAmount = u.TotalAmount,
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
                            Razduzenje = p.TotalAmount - p.Razduzeno,
                            Zaduzenje = 0,
                            TotalAmount = p.TotalAmount,
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
                            Razduzenje = p.TotalAmount - p.Razduzeno,
                            Zaduzenje = 0,
                            TotalAmount = p.TotalAmount,
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
                            Razduzenje = p.TotalAmount - p.Razduzeno,
                            Zaduzenje = 0,
                            TotalAmount = p.TotalAmount,
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

        //private async Task<List<FinancialCardItemResponse>> GetAllClanarice(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var clanarine = _context.Zaduzenja.Where(z => z.UserId == id &&
        //        z.Date >= from.Date && z.Date <= to &&
        //        z.Type == (int)FinancialCardTypeEnumeration.Clanarina);

        //        if (clanarine.Any())
        //        {
        //            foreach (var c in clanarine)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = c.Id,
        //                    Type = FinancialCardTypeEnumeration.Clanarina,
        //                    Date = c.Date,
        //                    Razduzenje = c.Placeno,
        //                    Zaduzenje = c.TotalAmount,
        //                    Otpis = c.Otpis,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllOtpisPozajmice(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var clanarine = _context.Zaduzenja.Where(z => z.UserId == id &&
        //        z.Date >= from.Date && z.Date <= to &&
        //        z.Type == (int)FinancialCardTypeEnumeration.OtpisPozajmice);

        //        if (clanarine.Any())
        //        {
        //            foreach (var c in clanarine)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = c.Id,
        //                    Type = FinancialCardTypeEnumeration.OtpisPozajmice,
        //                    Date = c.Date,
        //                    Razduzenje = c.Placeno,
        //                    Zaduzenje = c.TotalAmount,
        //                    Otpis = c.Otpis,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllTermins(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var termins = _context.Termins.Where(t => t.UserId == id &&
        //        t.StartDateTime >= from && t.StartDateTime <= to &&
        //        t.Price != 0);

        //        if (termins.Any())
        //        {
        //            foreach(var t in termins )
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = t.Id,
        //                    Type = FinancialCardTypeEnumeration.Termini,
        //                    Date = t.StartDateTime,
        //                    Razduzenje = t.Placeno,
        //                    Zaduzenje = t.Price,
        //                    Otpis = t.Otpis,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllKafic(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var kafic = _context.Racuns.Where(r => r.UserId == id &&
        //        r.Date >= from && r.Date <= to &&
        //        r.Type == (int)FinancialCardTypeEnumeration.Kafic);

        //        if (kafic.Any())
        //        {
        //            foreach (var r in kafic)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = r.Id,
        //                    Type = FinancialCardTypeEnumeration.Kafic,
        //                    Date = r.Date,
        //                    Razduzenje = r.Placeno,
        //                    Pretplata = r.Pretplata,
        //                    Zaduzenje = r.TotalAmount,
        //                    Otpis = r.Otpis,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllProdavnica(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var kafic = _context.Racuns.Where(r => r.UserId == id &&
        //        r.Date >= from && r.Date <= to &&
        //        r.Type == (int)FinancialCardTypeEnumeration.Prodavnica);

        //        if (kafic.Any())
        //        {
        //            foreach (var r in kafic)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = r.Id,
        //                    Type = FinancialCardTypeEnumeration.Prodavnica,
        //                    Date = r.Date,
        //                    Razduzenje = r.Placeno,
        //                    Pretplata = r.Pretplata,
        //                    Zaduzenje = r.TotalAmount,
        //                    Otpis = r.Otpis,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllKotizacija(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var kafic = _context.Zaduzenja.Where(r => r.UserId == id &&
        //        r.Date >= from && r.Date <= to &&
        //        r.Type == (int)FinancialCardTypeEnumeration.Kotizacije);

        //        if (kafic.Any())
        //        {
        //            foreach (var r in kafic)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = r.Id,
        //                    Type = FinancialCardTypeEnumeration.Kotizacije,
        //                    Date = r.Date,
        //                    Razduzenje = r.Placeno,
        //                    Zaduzenje = r.TotalAmount,
        //                    Otpis = r.Otpis,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllUplate(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var uplate = _context.Uplata.Where(u => u.UserId == id && 
        //        u.Date >= from && u.Date <= to &&
        //        u.TypeUplata == (int)UplataEnumeration.Standard);

        //        if (uplate.Any())
        //        {
        //            foreach(var u in uplate )
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = u.Id,
        //                    Type = FinancialCardTypeEnumeration.Uplate,
        //                    Date = u.Date,
        //                    Razduzenje = u.TotalAmount,
        //                    Zaduzenje = 0,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllPoklon(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var poklon = _context.Uplata.Where(p => p.UserId == id && 
        //        p.Date >= from && p.Date <= to && 
        //        p.TypeUplata == (int)UplataEnumeration.Poklon);

        //        if (poklon.Any())
        //        {
        //            foreach (var p in poklon)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = p.Id,
        //                    Type = FinancialCardTypeEnumeration.Poklon,
        //                    Date = p.Date,
        //                    Razduzenje = p.TotalAmount,
        //                    Zaduzenje = 0,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllPozajmica(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var poklon = _context.Uplata.Where(p => p.UserId == id && p.Date >= from &&
        //        p.Date <= to &&
        //        p.TypeUplata == (int)UplataEnumeration.Pozajmica);

        //        if (poklon.Any())
        //        {
        //            foreach (var p in poklon)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = p.Id,
        //                    Type = FinancialCardTypeEnumeration.Pozajmica,
        //                    Date = p.Date,
        //                    Razduzenje = p.TotalAmount,
        //                    Zaduzenje = 0,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}
        //private async Task<List<FinancialCardItemResponse>> GetAllOtkazTermina(int id, DateTime from, DateTime to)
        //{
        //    List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

        //    try
        //    {
        //        var otkazTermina = _context.Uplata.Where(p => p.UserId == id && p.Date >= from &&
        //        p.Date <= to &&
        //        p.TypeUplata == (int)UplataEnumeration.OtkazTermina);

        //        if (otkazTermina.Any())
        //        {
        //            foreach (var p in otkazTermina)
        //            {
        //                FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
        //                {
        //                    Id = p.Id,
        //                    Type = FinancialCardTypeEnumeration.OtkazTermina,
        //                    Date = p.Date,
        //                    Razduzenje = p.TotalAmount,
        //                    Zaduzenje = 0,
        //                };

        //                items.Add(financialCardItemResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }

        //    return items;
        //}

    }
}
