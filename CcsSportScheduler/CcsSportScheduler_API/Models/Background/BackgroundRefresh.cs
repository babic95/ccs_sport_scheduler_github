using CcsSportScheduler_API.Enumeration;
using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

namespace CcsSportScheduler_API.Models.Background
{
    public class BackgroundRefresh : IHostedService, IDisposable
    {
        private Timer? _timerSat;
        private Timer? _timerDan;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundRefresh> _logger;
        private readonly int satiObavestenja = 3;

        public BackgroundRefresh(IServiceProvider serviceProvider, ILogger<BackgroundRefresh> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundRefresh service is starting.");

            DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
            DateTime nextFullHour = now.AddHours(1).Date.AddHours(now.Hour + 1);
            TimeSpan timeToWait = nextFullHour - now;

            _timerSat = new Timer(PoslednjeObavestenje, null, timeToWait, TimeSpan.FromHours(1));

            DateTime next7AM = now.Date.AddDays(now.Hour >= 7 ? 1 : 0).AddHours(7);
            TimeSpan timeToWait7AM = next7AM - now;

            _timerDan = new Timer(PrvoObavestenje, null, timeToWait7AM, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void PoslednjeObavestenje(object? state)
        {
            _logger.LogInformation("Executing PoslednjeObavestenje at {Time}", DateTime.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<SportSchedulerContext>();

                try
                {
                    DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                    var terminiObavestenja = _context.Termins.AsEnumerable()
                        .Where(t => t.UserId != null &&
                        t.StartDateTime.Date == now.Date &&
                        (t.StartDateTime - now).Ticks < new TimeOnly(satiObavestenja, 0).Ticks).ToList();

                    string obavestenjeText = $"Poštovani korisniče, Vaš termin počinje za {satiObavestenja}h. " +
                        $"Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije za 30min.";

                    if (terminiObavestenja != null &&
                        terminiObavestenja.Any())
                    {
                        foreach (var t in terminiObavestenja)
                        {
                            var vremeDoTermina = t.StartDateTime.Subtract(now).Ticks;
                            var aa = new TimeOnly(satiObavestenja, 0).Ticks;

                            var obavestenjeDB = await _context.Obavestenjas
                                .FirstOrDefaultAsync(o => o.TerminId == t.Id && o.PrvoSlanje == (int)ObavestenjaEnumeration.PoslednjeObavestenje);

                            if (obavestenjeDB == null)
                            {
                                obavestenjeDB = new Obavestenja()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Date = now,
                                    TerminId = t.Id,
                                    UserId = t.UserId.Value,
                                    Description = Encoding.UTF8.GetBytes(obavestenjeText),
                                    PrvoSlanje = (int)ObavestenjaEnumeration.PoslednjeObavestenje
                                };

                                _context.Obavestenjas.Add(obavestenjeDB);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing PoslednjeObavestenje");
                }
            }
        }

        private async void PrvoObavestenje(object? state)
        {
            _logger.LogInformation("Executing PrvoObavestenje at {Time}", DateTime.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<SportSchedulerContext>();

                try
                {
                    DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now,
                        TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));

                    var terminiObavestenja = _context.Termins.AsEnumerable()
                        .Where(t => t.UserId != null && 
                        t.StartDateTime.Date == now.Date)
                        .ToList();

                    foreach (var t in terminiObavestenja)
                    {
                        var obavestenjeDB = await _context.Obavestenjas
                            .FirstOrDefaultAsync(o => o.TerminId == t.Id);

                        if (obavestenjeDB == null)
                        {
                            string obavestenjeText = $"Poštovani korisniče, danas imate termin u {t.StartDateTime.Hour}:{t.StartDateTime.Minute}. " +
                                $"Ukoliko želite da otkažete termin bez naknade molimo Vas da to učinite najkasnije do {t.StartDateTime.Hour - satiObavestenja}:{t.StartDateTime.Minute - 30}.";

                            obavestenjeDB = new Obavestenja()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Date = now,
                                TerminId = t.Id,
                                UserId = t.UserId.Value,
                                Description = Encoding.UTF8.GetBytes(obavestenjeText),
                                PrvoSlanje = (int)ObavestenjaEnumeration.PrvoSlanje
                            };

                            _context.Obavestenjas.Add(obavestenjeDB);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing PrvoObavestenje");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundRefresh service is stopping.");

            _timerSat?.Change(Timeout.Infinite, 0);
            _timerDan?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timerSat?.Dispose();
            _timerDan?.Dispose();
        }
    }

}
