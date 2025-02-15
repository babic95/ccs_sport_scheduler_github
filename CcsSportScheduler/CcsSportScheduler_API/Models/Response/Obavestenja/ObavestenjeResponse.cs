using CcsSportScheduler_Database.Models;
using System.Text;

namespace CcsSportScheduler_API.Models.Response.Obavestenja
{
    public class ObavestenjeResponse
    {
        public ObavestenjeResponse() { }
        public ObavestenjeResponse(CcsSportScheduler_Database.Models.Obavestenja obavestenjeDB)
        {
            Id = obavestenjeDB.Id;
            TerminId = obavestenjeDB.TerminId;
            UserId = obavestenjeDB.UserId;
            Date = obavestenjeDB.Date;
            Seen = obavestenjeDB.Seen == 0 ? false : true;
            Description = Encoding.UTF8.GetString(obavestenjeDB.Description);
        }

        public string? Id { get; set; }
        public string? TerminId { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public bool Seen { get; set; }
    }
}
