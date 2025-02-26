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

            var userDB = await _context.Users.FirstOrDefaultAsync(u => u.KlubId == user.KlubId &&
            u.Jmbg == user.Jmbg);

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

            if (user.KlubId == null ||
                user.Type == null ||
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
                    Type = user.Type.Value,
                    Pol = user.Pol,
                };

                await _context.Users.AddAsync(userDB);
                await _context.SaveChangesAsync();
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

            return Ok();
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

                var termini = await GetAllTermins(id, from, to);
                var racuni = await GetAllRacuni(id, from, to);
                var uplate = await GetAllUplate(id, from, to);
                var pokloni = await GetAllPoklon(id, from, to);

                var items = new List<FinancialCardItemResponse>();

                if (type == null ||
                    type == -1)
                {
                    items.AddRange(termini);
                    items.AddRange(racuni);
                    items.AddRange(uplate);
                    items.AddRange(pokloni);

                    financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate ||
                    i.Type == FinancialCardTypeEnumeration.Poklon).Sum(u => u.Razduzenje);

                    financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Racuni ||
                    i.Type == FinancialCardTypeEnumeration.Termini).Sum(t => t.Zaduzenje);
                }
                else
                {
                    if (type == 1)
                    {
                        items.AddRange(termini);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Termini).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Termini).Sum(t => t.Zaduzenje);
                    }
                    if (type == 0)
                    {
                        items.AddRange(racuni);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Racuni).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Racuni).Sum(t => t.Zaduzenje);
                    }
                    if (type == 2)
                    {
                        items.AddRange(uplate);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Uplate).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = 0;
                    }
                    if (type == 3)
                    {
                        items.AddRange(pokloni);

                        financialCardResponse.TotalRazduzenje = items.Where(i => i.Type == FinancialCardTypeEnumeration.Poklon).Sum(u => u.Razduzenje);
                        financialCardResponse.TotalZaduzenje = 0;
                    }
                }

                financialCardResponse.TotalCount = items.Count;

                var neplaceni = items.Where(i => i.Razduzenje != i.Zaduzenje);
                var placeni = items.Where(i => i.Razduzenje == i.Zaduzenje);

                if(neplaceni.Any())
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

        private async Task<List<FinancialCardItemResponse>> GetAllTermins(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var termins = _context.Termins.Where(t => t.UserId == id && t.StartDateTime >= from && t.StartDateTime <= to);

                if (termins.Any())
                {
                    await termins.ForEachAsync(t =>
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
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }

        private async Task<List<FinancialCardItemResponse>> GetAllRacuni(int id, DateTime from, DateTime to)
        {
            List<FinancialCardItemResponse> items = new List<FinancialCardItemResponse>();

            try
            {
                var racuni = _context.Racuns.Where(r => r.UserId == id && r.Date >= from && r.Date <= to);

                if (racuni.Any())
                {
                    await racuni.ForEachAsync(r =>
                    {
                        FinancialCardItemResponse financialCardItemResponse = new FinancialCardItemResponse()
                        {
                            Id = r.Id,
                            Type = FinancialCardTypeEnumeration.Racuni,
                            Date = r.Date,
                            Razduzenje = r.Placeno,
                            Zaduzenje = r.TotalAmount,
                            Otpis = r.Otpis,
                        };

                        items.Add(financialCardItemResponse);
                    });
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
                var uplate = _context.Uplata.Where(u => u.UserId == id && u.Date >= from && u.Date <= to && u.TypeUplata == (int)UplataEnumeration.Standard);

                if (uplate.Any())
                {
                    await uplate.ForEachAsync(u =>
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
                    });
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
                var poklon = _context.Uplata.Where(p => p.UserId == id && p.Date >= from && p.Date <= to && p.TypeUplata == (int)UplataEnumeration.Poklon);

                if (poklon.Any())
                {
                    await poklon.ForEachAsync(p =>
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
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return items;
        }
    }
}
