using Azure.Core;
using CcsSportScheduler_API.Enumeration;
using CcsSportScheduler_API.Models;
using CcsSportScheduler_API.Models.Requests.Report;
using CcsSportScheduler_API.Models.Response;
using CcsSportScheduler_API.Models.Response.FinancialCard;
using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace CcsSportScheduler_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : Controller
    {

        private readonly SportSchedulerContext _context;

        public ReportController(SportSchedulerContext context)
        {
            _context = context;
        }

        [Route("createAll")]
        [HttpPost]
        public async Task<IActionResult> CreateAll([FromBody] ReportRequest request)
        {
            DateTime from = TimeZoneInfo.ConvertTime(request.FromDate,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

            DateTime to = TimeZoneInfo.ConvertTime(request.ToDate,
                TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

            from = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0);
            to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);

            List<ReportUser> reportUsers = new List<ReportUser>();

            foreach (var userDB in _context.Users)
            {
                var clanarice = await GetAllClanarice(userDB.Id, from, to);
                var termini = await GetAllTermins(userDB.Id, from, to);
                var kafic = await GetAllKafic(userDB.Id, from, to);
                var prodavnica = await GetAllProdavnica(userDB.Id, from, to);
                var kotizacije = await GetAllKotizacija(userDB.Id, from, to);
                var otpisPozajmice = await GetAllOtpisPozajmice(userDB.Id, from, to);
                var uplate = await GetAllUplate(userDB.Id, from, to);
                var pokloni = await GetAllPoklon(userDB.Id, from, to);
                var pozajmica = await GetAllPozajmica(userDB.Id, from, to);

                var reportUser = new ReportUser()
                {
                    UserId = userDB.Id,
                    Username = userDB.Username,
                    FullName = userDB.FullName,
                    Items = new List<RportUserItems>(),
                    TotalZaduzenje = 0,
                    TotalRazduzenje = 0,
                    TotalOtpis = 0,
                    TotalSaldo = 0
                };

                RportUserItems clanarineReport = new RportUserItems()
                {
                    Name = "Članarine:",
                    Zaduzenje = clanarice.Sum(c => c.Zaduzenje),
                    Otpis = clanarice.Sum(c => c.Otpis),
                    Razduzenje = clanarice.Sum(c => c.Razduzenje),
                    Saldo = clanarice.Sum(c => c.Razduzenje) + clanarice.Sum(c => c.Otpis) - clanarice.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(clanarineReport);

                RportUserItems terminiReport = new RportUserItems()
                {
                    Name = "Termini:",
                    Zaduzenje = termini.Sum(c => c.Zaduzenje),
                    Otpis = termini.Sum(c => c.Otpis),
                    Razduzenje = termini.Sum(c => c.Razduzenje),
                    Saldo = termini.Sum(c => c.Razduzenje) + termini.Sum(c => c.Otpis) - termini.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(terminiReport);

                RportUserItems kaficReport = new RportUserItems()
                {
                    Name = "Kafic:",
                    Zaduzenje = kafic.Sum(c => c.Zaduzenje),
                    Otpis = kafic.Sum(c => c.Otpis),
                    Razduzenje = kafic.Sum(c => c.Razduzenje),
                    Saldo = kafic.Sum(c => c.Razduzenje) + kafic.Sum(c => c.Otpis) - kafic.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(kaficReport);

                RportUserItems prodavnicaReport = new RportUserItems()
                {
                    Name = "Prodavnica:",
                    Zaduzenje = prodavnica.Sum(c => c.Zaduzenje),
                    Otpis = prodavnica.Sum(c => c.Otpis),
                    Razduzenje = prodavnica.Sum(c => c.Razduzenje),
                    Saldo = prodavnica.Sum(c => c.Razduzenje) + prodavnica.Sum(c => c.Otpis) - prodavnica.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(prodavnicaReport);

                RportUserItems kotizacijeReport = new RportUserItems()
                {
                    Name = "Kotizacije:",
                    Zaduzenje = kotizacije.Sum(c => c.Zaduzenje),
                    Otpis = kotizacije.Sum(c => c.Otpis),
                    Razduzenje = kotizacije.Sum(c => c.Razduzenje),
                    Saldo = kotizacije.Sum(c => c.Razduzenje) + kotizacije.Sum(c => c.Otpis) - kotizacije.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(kotizacijeReport);

                RportUserItems otpisPozajmiceReport = new RportUserItems()
                {
                    Name = "Otpis Pozajmice:",
                    Zaduzenje = otpisPozajmice.Sum(c => c.Zaduzenje),
                    Otpis = otpisPozajmice.Sum(c => c.Otpis),
                    Razduzenje = otpisPozajmice.Sum(c => c.Razduzenje),
                    Saldo = otpisPozajmice.Sum(c => c.Razduzenje) + otpisPozajmice.Sum(c => c.Otpis) - otpisPozajmice.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(otpisPozajmiceReport);

                RportUserItems uplateReport = new RportUserItems()
                {
                    Name = "Uplate:",
                    Zaduzenje = uplate.Sum(c => c.Zaduzenje),
                    Otpis = uplate.Sum(c => c.Otpis),
                    Razduzenje = uplate.Sum(c => c.Razduzenje),
                    Saldo = uplate.Sum(c => c.Razduzenje) + uplate.Sum(c => c.Otpis) - uplate.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(uplateReport);

                RportUserItems pokloniReport = new RportUserItems()
                {
                    Name = "Otpis:",
                    Zaduzenje = pokloni.Sum(c => c.Zaduzenje),
                    Otpis = pokloni.Sum(c => c.Otpis),
                    Razduzenje = pokloni.Sum(c => c.Razduzenje),
                    Saldo = pokloni.Sum(c => c.Razduzenje) + pokloni.Sum(c => c.Otpis) - pokloni.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(pokloniReport);

                RportUserItems pozajmicaReport = new RportUserItems()
                {
                    Name = "Pozajmice:",
                    Zaduzenje = pozajmica.Sum(c => c.Zaduzenje),
                    Otpis = pozajmica.Sum(c => c.Otpis),
                    Razduzenje = pozajmica.Sum(c => c.Razduzenje),
                    Saldo = pozajmica.Sum(c => c.Razduzenje) + pozajmica.Sum(c => c.Otpis) - pozajmica.Sum(c => c.Zaduzenje)
                };
                reportUser.Items.Add(pozajmicaReport);

                reportUser.TotalOtpis = reportUser.Items.Sum(i => i.Otpis);
                reportUser.TotalRazduzenje = reportUser.Items.Sum(i => i.Razduzenje);
                reportUser.TotalZaduzenje = reportUser.Items.Sum(i => i.Zaduzenje);
                reportUser.TotalSaldo = reportUser.TotalRazduzenje + reportUser.TotalOtpis - reportUser.TotalZaduzenje;

                reportUsers.Add(reportUser);
            }

            var pdf = await CreatePdf(reportUsers, from, to);

            return File(pdf, "application/pdf", "report.pdf");
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportRequest request)
        {
            var userDB = await _context.Users.FindAsync(request.UserId);

            if (userDB == null)
            {
                return NotFound(new ErrorResponse
                {
                    Controller = "ReportController",
                    Message = "Ne postoji korisnik u bazi podataka.",
                    Code = ErrorEnumeration.NotFound,
                    Action = "Create"
                });
            }

            DateTime from = TimeZoneInfo.ConvertTime(request.FromDate,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

            DateTime to = TimeZoneInfo.ConvertTime(request.ToDate,
                TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

            from = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0);
            to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59);

            var clanarice = await GetAllClanarice(request.UserId, from, to);
            var termini = await GetAllTermins(request.UserId, from, to);
            var kafic = await GetAllKafic(request.UserId, from, to);
            var prodavnica = await GetAllProdavnica(request.UserId, from, to);
            var kotizacije = await GetAllKotizacija(request.UserId, from, to);
            var otpisPozajmice = await GetAllOtpisPozajmice(request.UserId, from, to);
            var uplate = await GetAllUplate(request.UserId, from, to);
            var pokloni = await GetAllPoklon(request.UserId, from, to);
            var pozajmica = await GetAllPozajmica(request.UserId, from, to);

            List<ReportUser> reportUsers = new List<ReportUser>();

            var reportUser = new ReportUser()
            {
                UserId = userDB.Id,
                Username = userDB.Username,
                FullName = userDB.FullName,
                Items = new List<RportUserItems>(),
                TotalZaduzenje = 0,
                TotalRazduzenje = 0,
                TotalOtpis = 0,
                TotalSaldo = 0
            };

            RportUserItems clanarineReport = new RportUserItems()
            {
                Name = "Članarine:",
                Zaduzenje = clanarice.Sum(c => c.Zaduzenje),
                Otpis = clanarice.Sum(c => c.Otpis),
                Razduzenje = clanarice.Sum(c => c.Razduzenje),
                Saldo = clanarice.Sum(c => c.Razduzenje) + clanarice.Sum(c => c.Otpis) - clanarice.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(clanarineReport);

            RportUserItems terminiReport = new RportUserItems()
            {
                Name = "Termini:",
                Zaduzenje = termini.Sum(c => c.Zaduzenje),
                Otpis = termini.Sum(c => c.Otpis),
                Razduzenje = termini.Sum(c => c.Razduzenje),
                Saldo = termini.Sum(c => c.Razduzenje) + termini.Sum(c => c.Otpis) - termini.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(terminiReport);

            RportUserItems kaficReport = new RportUserItems()
            {
                Name = "Kafic:",
                Zaduzenje = kafic.Sum(c => c.Zaduzenje),
                Otpis = kafic.Sum(c => c.Otpis),
                Razduzenje = kafic.Sum(c => c.Razduzenje),
                Saldo = kafic.Sum(c => c.Razduzenje) + kafic.Sum(c => c.Otpis) - kafic.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(kaficReport);

            RportUserItems prodavnicaReport = new RportUserItems()
            {
                Name = "Prodavnica:",
                Zaduzenje = prodavnica.Sum(c => c.Zaduzenje),
                Otpis = prodavnica.Sum(c => c.Otpis),
                Razduzenje = prodavnica.Sum(c => c.Razduzenje),
                Saldo = prodavnica.Sum(c => c.Razduzenje) + prodavnica.Sum(c => c.Otpis) - prodavnica.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(prodavnicaReport);

            RportUserItems kotizacijeReport = new RportUserItems()
            {
                Name = "Kotizacije:",
                Zaduzenje = kotizacije.Sum(c => c.Zaduzenje),
                Otpis = kotizacije.Sum(c => c.Otpis),
                Razduzenje = kotizacije.Sum(c => c.Razduzenje),
                Saldo = kotizacije.Sum(c => c.Razduzenje) + kotizacije.Sum(c => c.Otpis) - kotizacije.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(kotizacijeReport);

            RportUserItems otpisPozajmiceReport = new RportUserItems()
            {
                Name = "Otpis Pozajmice:",
                Zaduzenje = otpisPozajmice.Sum(c => c.Zaduzenje),
                Otpis = otpisPozajmice.Sum(c => c.Otpis),
                Razduzenje = otpisPozajmice.Sum(c => c.Razduzenje),
                Saldo = otpisPozajmice.Sum(c => c.Razduzenje) + otpisPozajmice.Sum(c => c.Otpis) - otpisPozajmice.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(otpisPozajmiceReport);

            RportUserItems uplateReport = new RportUserItems()
            {
                Name = "Uplate:",
                Zaduzenje = uplate.Sum(c => c.Zaduzenje),
                Otpis = uplate.Sum(c => c.Otpis),
                Razduzenje = uplate.Sum(c => c.Razduzenje),
                Saldo = uplate.Sum(c => c.Razduzenje) + uplate.Sum(c => c.Otpis) - uplate.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(uplateReport);

            RportUserItems pokloniReport = new RportUserItems()
            {
                Name = "Otpis:",
                Zaduzenje = pokloni.Sum(c => c.Zaduzenje),
                Otpis = pokloni.Sum(c => c.Otpis),
                Razduzenje = pokloni.Sum(c => c.Razduzenje),
                Saldo = pokloni.Sum(c => c.Razduzenje) + pokloni.Sum(c => c.Otpis) - pokloni.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(pokloniReport);

            RportUserItems pozajmicaReport = new RportUserItems()
            {
                Name = "Pozajmice:",
                Zaduzenje = pozajmica.Sum(c => c.Zaduzenje),
                Otpis = pozajmica.Sum(c => c.Otpis),
                Razduzenje = pozajmica.Sum(c => c.Razduzenje),
                Saldo = pozajmica.Sum(c => c.Razduzenje) + pozajmica.Sum(c => c.Otpis) - pozajmica.Sum(c => c.Zaduzenje)
            };
            reportUser.Items.Add(pozajmicaReport);

            reportUser.TotalOtpis = reportUser.Items.Sum(i => i.Otpis);
            reportUser.TotalRazduzenje = reportUser.Items.Sum(i => i.Razduzenje);
            reportUser.TotalZaduzenje = reportUser.Items.Sum(i => i.Zaduzenje);
            reportUser.TotalSaldo = reportUser.TotalRazduzenje + reportUser.TotalOtpis - reportUser.TotalZaduzenje;

            reportUsers.Add(reportUser);
            var pdf = await CreatePdf(reportUsers, from, to);

            return File(pdf, "application/pdf", "report.pdf");
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

        private async Task<byte[]> CreatePdf(List<ReportUser> reportUsers, DateTime fromDate, DateTime toDate)
        {
            using (var stream = new MemoryStream())
            {
                PdfDocument document = new PdfDocument();
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Verdana", 12, XFontStyle.Regular);
                XFont boldFont = new XFont("Verdana", 12, XFontStyle.Bold);

                int yOffset = 60;
                gfx.DrawString($"Izveštaj: {fromDate.ToShortDateString()} - {toDate.ToShortDateString()}", boldFont, XBrushes.Black, new XRect(0, yOffset, page.Width, 50), XStringFormats.TopCenter);

                yOffset += 60;
                foreach (var report in reportUsers)
                {
                    int tableStartX = 20;
                    gfx.DrawString($"Član: {report.FullName} ({report.Username}):", boldFont, XBrushes.Black, new XRect(tableStartX, yOffset, page.Width, 20), XStringFormats.TopLeft);
                    yOffset += 20;

                    int tableStartY = yOffset;
                    int tableWidth = (int)page.Width - 40;

                    // Draw table header
                    gfx.DrawRectangle(XPens.Black, tableStartX, tableStartY, tableWidth, 20);
                    gfx.DrawString("Naziv", boldFont, XBrushes.Black, new XRect(tableStartX, tableStartY, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString("Zaduzenje", boldFont, XBrushes.Black, new XRect(tableStartX + tableWidth / 5, tableStartY, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString("Razduzenje", boldFont, XBrushes.Black, new XRect(tableStartX + 2 * tableWidth / 5, tableStartY, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString("Otpis", boldFont, XBrushes.Black, new XRect(tableStartX + 3 * tableWidth / 5, tableStartY, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString("Saldo", boldFont, XBrushes.Black, new XRect(tableStartX + 4 * tableWidth / 5, tableStartY, tableWidth / 5, 20), XStringFormats.Center);

                    yOffset += 20;

                    // Draw table rows
                    foreach (var item in report.Items)
                    {
                        gfx.DrawRectangle(XPens.Black, tableStartX, yOffset, tableWidth, 20);
                        gfx.DrawString(item.Name, font, XBrushes.Black, new XRect(tableStartX, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                        gfx.DrawString(item.Zaduzenje.ToString(), font, XBrushes.Black, new XRect(tableStartX + tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                        gfx.DrawString(item.Razduzenje.ToString(), font, XBrushes.Black, new XRect(tableStartX + 2 * tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                        gfx.DrawString(item.Otpis.ToString(), font, XBrushes.Black, new XRect(tableStartX + 3 * tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                        gfx.DrawString(item.Saldo.ToString(), font, XBrushes.Black, new XRect(tableStartX + 4 * tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);

                        yOffset += 20;

                        if (yOffset > page.Height - 50) // Check if we need a new page
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yOffset = 20;
                        }
                    }

                    // Draw totals row
                    gfx.DrawRectangle(XPens.Black, tableStartX, yOffset, tableWidth, 20);
                    gfx.DrawString("UKUPNO:", boldFont, XBrushes.Black, new XRect(tableStartX, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString(report.TotalZaduzenje.ToString(), boldFont, XBrushes.Black, new XRect(tableStartX + tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString(report.TotalRazduzenje.ToString(), boldFont, XBrushes.Black, new XRect(tableStartX + 2 * tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString(report.TotalOtpis.ToString(), boldFont, XBrushes.Black, new XRect(tableStartX + 3 * tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);
                    gfx.DrawString(report.TotalSaldo.ToString(), boldFont, XBrushes.Black, new XRect(tableStartX + 4 * tableWidth / 5, yOffset, tableWidth / 5, 20), XStringFormats.Center);

                    yOffset += 40;
                    if (yOffset > page.Height - 50) // Check if we need a new page
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yOffset = 20;
                    }
                }

                document.Save(stream, false);
                return stream.ToArray();
            }
        }
    }
}