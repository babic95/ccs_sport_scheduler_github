using CcsSportScheduler_API.Enumeration;
using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CcsSportScheduler_API.Models.Background
{
    public class AddNewFreeTerminBackground : IHostedService, IDisposable
    {
        private Timer? _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AddNewFreeTerminBackground> _logger;

        public AddNewFreeTerminBackground(IServiceProvider serviceProvider, ILogger<AddNewFreeTerminBackground> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddNewFreeTerminBackground service is starting.");

            ScheduleNextExecution();

            return Task.CompletedTask;
        }

        private void ScheduleNextExecution()
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
            DateTime nextExecution = new DateTime(now.Year, now.Month, 1, 3, 0, 0);

            if (now > nextExecution)
            {
                nextExecution = nextExecution.AddMonths(1);
            }

            TimeSpan timeToWait = nextExecution - now;
            _timer = new Timer(AddNewFreeTermin, null, timeToWait, TimeSpan.FromMilliseconds(-1)); // Execute once at the specified time
        }

        private async void AddNewFreeTermin(object? state)
        {
            _logger.LogInformation("Executing AddNewFreeTermin at {Time}", DateTime.Now);

            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<SportSchedulerContext>();

                try
                {
                    var users = _context.Users.Where(u => u.Type == (int)UserEnumeration.Plivajuci);

                    foreach (var user in users)
                    {
                        user.FreeTermin = 4;
                        _context.Users.Update(user);
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully added 4 free termin to all eligible users.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing AddNewFreeTermin");
                }
            }

            // Schedule the next execution
            ScheduleNextExecution();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddNewFreeTerminBackground service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}