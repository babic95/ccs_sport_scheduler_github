using CcsSportScheduler_Database;
using CcsSportScheduler_Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CcsSportScheduler_API.Services
{
    public class NaplataTerminaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SportSchedulerContext _context;

        public NaplataTerminaService(IHttpClientFactory httpClientFactory, SportSchedulerContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<List<NaplataTermina>> GetNaplataTerminaAsync(int klubId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"/api/klubs/naplataTermina/{klubId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<NaplataTermina>>();
            }
            else
            {
                throw new HttpRequestException("Greška prilikom dohvatanja cena termina.");
            }
        }
    }
}
