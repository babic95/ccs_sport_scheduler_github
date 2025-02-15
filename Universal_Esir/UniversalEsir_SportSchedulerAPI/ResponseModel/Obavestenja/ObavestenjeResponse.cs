using System.Text;

namespace UniversalEsir_SportSchedulerAPI.ResponseModel.Obavestenja
{
    public class ObavestenjeResponse
    {
        public ObavestenjeResponse() { }

        public string? Id { get; set; }
        public string? TerminId { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public bool Seen { get; set; }
    }
}
